using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;
using Python.Core.Expressions;
namespace Python.Parser
{
    public class OperatorSubParser
    {
        public PythonParser Parser { get; set; }
        public OperatorSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //simple_stmt:
        //    | assignment
        //    | star_expressions 
        //    | return_stmt
        //    | import_stmt
        //    | raise_stmt
        //    | 'pass' 
        //    | del_stmt
        //    | yield_stmt
        //    | assert_stmt
        //    | 'break' 
        //    | 'continue' 
        //    | global_stmt
        //    | nonlocal_stmt
        public Expression ParseSimpleStmt()
        {
            string next = Parser.Peek().Value;
            if (next == Keyword.Nonlocal.Value)
            {
                Parser.Advance();
                // NAME should be one token
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(new SimpleExpression
                {
                    IsConstant = false,
                    IsVariable = true,
                    Value = Parser.Peek().Value
                });
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(new SimpleExpression
                    {
                        IsConstant = false,
                        IsVariable = true,
                        Value = Parser.Peek().Value
                    });
                    Parser.Advance();
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Nonlocal,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Global.Value)
            {
                Parser.Advance();
                // NAME should be one token
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(new SimpleExpression
                {
                    IsConstant = false,
                    IsVariable = true,
                    Value = Parser.Peek().Value
                });
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(new SimpleExpression
                    {
                        IsConstant = false,
                        IsVariable = true,
                        Value = Parser.Peek().Value
                    });
                    Parser.Advance();
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Global,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Continue.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Continue,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Break.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Break,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Assert.Value)
            {
                Parser.Advance();
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(Parser.ParseExpression());
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(Parser.ParseExpression());
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Assert,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Yield.Value)
            {
                return Parser.AtomSubParser.ParseYieldExpr();
            }
            // 'del' del_targets &(';' | NEWLINE) 
            if (next == Keyword.Del.Value)
            {
                Parser.Advance();
                CollectionExpression ex = ParseDelTargets();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Del,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Pass.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Pass,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Raise.Value)
            {
                Parser.Advance();
                if (Parser.Peek().Type != TokenType.EndOfExpression)
                {
                    Expression ex = Parser.ParseExpression();
                    if (Parser.Peek().Value == Keyword.From.Value)
                    {
                        Parser.Advance();
                        Expression src = Parser.ParseExpression();
                        return new RaiseExpression
                        {
                            Expression = ex,
                            Source = src
                        };
                    }
                    else
                    {
                        return new RaiseExpression
                        {
                            Expression = ex,
                            Source = null
                        };
                    }
                }
                else
                {
                    return new RaiseExpression();
                }
            }
            if (next == Keyword.From.Value || next == Keyword.Import.Value)
            {
                return ParseImportStmt();
            }
            if (next == Keyword.Return.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Return,
                    Operator = null,
                    Expression = Parser.ParseStarExpressions()
                };
            }
            int previous = Parser.Position;
            //try
            {
                Expression ex = ParseAssignment();
                if (ex == null)
                {
                    Parser.RewindTo(previous);
                    ex = Parser.ParseStarExpressions();
                }
                if (!Parser.HasErrors())
                {
                    return ex;
                }
            }
            //catch (Exception ex)
            {
                Parser.RewindTo(previous);
                return Parser.ParseStarExpressions();
            }
        }
        // del_targets: ','.del_target+ [',']
        public CollectionExpression ParseDelTargets()
        {
            CollectionExpression ex = new CollectionExpression
            {
                Type = CollectionType.Unknown
            };
            ex.Elements.Add(ParseDelTarget());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                ex.Elements.Add(ParseDelTarget());
            }
            return ex;
        }
        //  del_target:
        //   | t_primary '.' NAME !t_lookahead 
        //   | t_primary '[' slices ']' !t_lookahead 
        //   | del_t_atom
        // del_t_atom:
        //   | NAME 
        //   | '(' del_target ')' 
        //   | '(' [del_targets] ')' 
        //   | '[' [del_targets] ']'
        public Expression ParseDelTarget()
        {
            // try to parse as a t_primary first
            int position = Parser.Position;
            //try
            {
                Expression tprimary = Parser.AtomSubParser.ParseTPrimary();
                if (Parser.Peek().Value == ".")
                {
                    Parser.Advance();
                    string name = Parser.Peek().Value;
                    Parser.Advance();
                    Parser.AtomSubParser.DontAcceptTLookahead();
                    if (!Parser.HasErrors())
                    {
                        return new EvaluatedExpression
                        {
                            LeftHandValue = tprimary,
                            Operator = Operator.ObjectReference,
                            RightHandValue = new SimpleExpression
                            {
                                Value = name,
                                IsVariable = true
                            }
                        };
                    }
                }
                else if (Parser.Peek().Value == "[")
                {
                    Parser.Advance();
                    Expression slices = Parser.AtomSubParser.ParseSlices();
                    Parser.Accept("]");
                    Parser.Advance();
                    Parser.AtomSubParser.DontAcceptTLookahead();
                    if (!Parser.HasErrors())
                    {
                        return new EvaluatedExpression
                        {
                            LeftHandValue = tprimary,
                            IsArrayAccessor = true,
                            RightHandValue = slices
                        };
                    }
                }
                else
                {
                    if (!Parser.HasErrors())
                    {
                        return tprimary;
                    }
                }
            }
            //catch (Exception)
            {
                Parser.RewindTo(position);
                // it's not a t_primary; hopefully this try-catch isn't too expensive
                if (Parser.Peek().Type == TokenType.Variable)
                {
                    Expression ex = new SimpleExpression
                    {
                        IsVariable = true,
                        Value = Parser.Peek().Value
                    };
                    Parser.Advance();
                    return ex;
                }
                else if (Parser.Peek().Type == TokenType.BeginParameters)
                {
                    Parser.Advance();
                    Expression ex = ParseDelTargets();
                    Parser.Accept(TokenType.EndParameters);
                    Parser.Advance();
                    return ex;
                }
                else if(Parser.Peek().Type == TokenType.BeginList)
                {
                    Parser.Advance();
                    Expression ex = ParseDelTargets();
                    Parser.Accept(TokenType.EndList);
                    Parser.Advance();
                    return ex;
                }
                else
                {
                    Parser.ThrowSyntaxError(Parser.Position);
                }
            }
            return null; // shouldn't get past the syntax error
        }
        // import_stmt: import_name | import_from
        // import_name: 'import' dotted_as_names
        // # note below: the ('.' | '...') is necessary because '...' is tokenized as ELLIPSIS
        // import_from:
        //    | 'from' ('.' | '...')* dotted_name 'import' import_from_targets 
        //    | 'from' ('.' | '...')+ 'import' import_from_targets
        public Expression ParseImportStmt()
        {
            string next = Parser.Peek().Value;
            Parser.Advance();
            if (next == Keyword.Import.Value)
            {
                return new ImportExpression
                {
                    Imports = ParseDottedAsNames()
                };
            }
            else if (next == Keyword.From.Value)
            {
                string fromPath = ParseImportFromPath();
                Parser.Accept(Keyword.Import.Value);
                Parser.Advance();
                List<KeyValuePair<string, string>> targets = ParseImportFromTargets();
                return new ImportExpression
                {
                    Imports = targets.Select(t => new KeyValuePair<string, string>(t.Key, fromPath + "." + t.Value)).ToList()
                };
            }
            Parser.ThrowSyntaxError(Parser.Position);
            return null; // won't get here
        }
        public string ParseImportFromPath()
        {
            string value = string.Empty;
            // just grab all . characters (the tokenizer doesn't treat ELLIPSIS specially)
            while (Parser.Peek().Type == TokenType.ObjectReference || Parser.Peek().Value == ".")
            {
                value += ".";
                Parser.Advance();
            }
            if (Parser.Peek().Value != Keyword.Import.Value)
            {
                value += ParseDottedName();
            }
            return value;
        }
        //import_from_targets:
        //    | '(' import_from_as_names[','] ')' 
        //    | import_from_as_names !','
        //    | '*'
        public List<KeyValuePair<string, string>> ParseImportFromTargets()
        {
            List<KeyValuePair<string, string>> names;
            if (Parser.Peek().Value == "*")
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("*", "*")
                };
            }
            if (Parser.Peek().Value == "(" || Parser.Peek().Type == TokenType.BeginParameters)
            {
                Parser.Advance();
                names = ParseImportFromAsNames();
                Parser.Accept(TokenType.EndParameters);
                Parser.Advance();
                return names;
            }
            names = ParseImportFromAsNames();
            return names;
        }
        //import_from_as_names:
        //    | ','.import_from_as_name+ 
        public List<KeyValuePair<string, string>> ParseImportFromAsNames()
        {
            List<KeyValuePair<string, string>> imports = new List<KeyValuePair<string, string>>();
            imports.Add(ParseImportFromAsName());
            while (Parser.Peek().Value == "," || Parser.Peek().Type == TokenType.ElementSeparator)
            {
                Parser.Advance();
                imports.Add(ParseImportFromAsName());
            }
            return imports;
        }
        //dotted_as_names:
        //    | ','.dotted_as_name+ 
        public List<KeyValuePair<string, string>> ParseDottedAsNames()
        {
            List<KeyValuePair<string, string>> imports = new List<KeyValuePair<string, string>>();
            imports.Add(ParseDottedAsName());
            while (Parser.Peek().Value == "," || Parser.Peek().Type == TokenType.ElementSeparator)
            {
                Parser.Advance();
                imports.Add(ParseDottedAsName());
            }
            return imports;
        }
        //import_from_as_name:
        //    | NAME['as' NAME]
        public KeyValuePair<string, string> ParseImportFromAsName()
        {
            string importPath = ParseName();
            if (Parser.Peek().Value == Keyword.As.Value)
            {
                Parser.Advance();
                string importAlias = Parser.Peek().Value;
                Parser.Advance();
                return new KeyValuePair<string, string>(importAlias, importPath);
            }
            else
            {
                return new KeyValuePair<string, string>(importPath, importPath);
            }
        }
        public string ParseName()
        {
            Parser.Accept(TokenType.Variable);
            string name = Parser.Peek().Value;
            Parser.Advance();
            return name;
        }
        //dotted_as_name:
        //    | dotted_name['as' NAME]
        public KeyValuePair<string, string> ParseDottedAsName()
        {
            string importPath = ParseDottedName();
            if (Parser.Peek().Value == Keyword.As.Value)
            {
                Parser.Advance();
                string importAlias = Parser.Peek().Value;
                Parser.Advance();
                return new KeyValuePair<string, string>(importAlias, importPath);
            }
            else
            {
                return new KeyValuePair<string, string>(importPath, importPath);
            }
        }
        //dotted_name:
        //   | dotted_name '.' NAME 
        //   | NAME
        public string ParseDottedName()
        {
            Parser.Accept(TokenType.Variable);
            string name = Parser.Peek().Value;
            Parser.Advance();
            while (Parser.Peek().Value == "." || Parser.Peek().Type == TokenType.ObjectReference)
            {
                Parser.Advance();
                name += "." + Parser.Peek().Value;
                Parser.Advance();
            }
            return name;
        }
        //assignment:
        //    | NAME ':' expression['=' annotated_rhs] 
        //    | ('(' single_target ')' 
        //         | single_subscript_attribute_target) ':' expression['=' annotated_rhs] 
        //    | (star_targets '=' )+ (yield_expr | star_expressions) !'=' [TYPE_COMMENT] 
        //    | single_target augassign ~ (yield_expr | star_expressions) 
        //
        public Expression ParseAssignment()
        {
            // augmented assignment expression
            if (Parser.Peek().Type == TokenType.Variable
                && Parser.Peek(1).Value == ":")
            {
                int position = Parser.Position;
                //try
                {
                    string tgt = Parser.Peek().Value;
                    Parser.Advance();
                    Parser.Advance();
                    Expression ex = Parser.ParseExpression();
                    if (Parser.Peek().Value == "=")
                    {
                        Parser.Advance();
                        Expression rhs = ParseAnnotatedRhs();
                        if (!Parser.HasErrors())
                        {
                            return new EvaluatedExpression
                            {
                                LeftHandValue = new SimpleExpression
                                {
                                    Value = tgt,
                                    IsVariable = true
                                },
                                Annotation = ex,
                                Operator = Operator.Set,
                                RightHandValue = rhs
                            };
                        }
                    }
                    else
                    {
                        if (!Parser.HasErrors())
                        {
                            // the RHS is optional in this assignment? leave it as a simple expression for now
                            return new SimpleExpression
                            {
                                Value = tgt,
                                IsVariable = true,
                                Annotation = ex
                            };
                        }
                    }
                }
                //catch (Exception ex)
                {
                    Parser.RewindTo(position);
                    // rewind and try the next one
                }
            }
            if (Parser.Peek().Value == "(")
            {
                int position = Parser.Position;
                //try
                {
                    // TODO the grammar makes this look like it should have a nested case?
                    Expression singleTarget = Parser.AtomSubParser.ParseSingleTarget();
                    Parser.Accept(")");
                    if (!Parser.HasErrors())
                    {
                        return singleTarget;
                    }
                }
                //catch (Exception ex)
                {
                    Parser.RewindTo(position);
                    // rewind and try the next one
                }
            }
            int previous = Parser.Position;
            //try
            {
                Expression lhs = Parser.AtomSubParser.ParseSingleSubscriptAttributeTarget();
                Parser.Accept(":");
                Expression ex = Parser.ParseExpression();
                if (Parser.Peek().Value == "=")
                {
                    Parser.Advance();
                    Expression rhs = ParseAnnotatedRhs();
                    if (!Parser.HasErrors())
                    {
                        return new EvaluatedExpression
                        {
                            LeftHandValue = lhs,
                            Annotation = ex,
                            Operator = Operator.Set,
                            RightHandValue = rhs
                        };
                    }
                }
                else
                {
                    if (!Parser.HasErrors())
                    {
                        // the RHS is optional in this assignment? leave it as a simple expression for now
                        return lhs;
                    }
                }
            }
            //catch (Exception ex)
            {
                Parser.RewindTo(previous);
                // rewind and try the next one
            }
            // star_targets
            previous = Parser.Position;
            //try
            {
                List<Expression> targets = new List<Expression>();
                Expression lhs = Parser.AtomSubParser.ParseStarTargets();
                Parser.Accept("=");
                Parser.Advance();
                targets.Add(lhs);
                while (Parser.IndexOf("=") < Parser.IndexOf(TokenType.EndOfExpression)
                        && Parser.IndexOf("=") >= 0)
                {
                    lhs = Parser.AtomSubParser.ParseStarTargets();
                    Parser.Accept("=");
                    Parser.Advance();
                    targets.Add(lhs);
                }
                Expression expr = Parser.Peek().Value == Keyword.Yield.Value ? Parser.AtomSubParser.ParseYieldExpr() : Parser.ParseStarExpressions();
                EvaluatedExpression result = new EvaluatedExpression
                {
                    LeftHandValue = targets[targets.Count - 1],
                    Operator = Operator.Set,
                    RightHandValue = expr
                };
                int idx = targets.Count - 2;
                while (idx >= 0)
                {
                    result = new EvaluatedExpression
                    {
                        LeftHandValue = targets[idx],
                        Operator = Operator.Set,
                        RightHandValue = result
                    };
                    idx--;
                }
                if (!Parser.HasErrors())
                {
                    return result;
                }
            }
            //catch (Exception ex)
            {
                Parser.RewindTo(previous);
                // rewind and try the next one
            }
            // single_target augassign
            previous = Parser.Position;
            Expression target = Parser.AtomSubParser.ParseSingleTarget();
            if (IsAugAssign(Parser.Peek().Value))
            {
                string op = Parser.Peek().Value;
                Parser.Advance();
                Expression expr = Parser.Peek().Value == Keyword.Yield.Value ? Parser.AtomSubParser.ParseYieldExpr() : Parser.ParseStarExpressions();
                return new EvaluatedExpression
                {
                    LeftHandValue = target,
                    Operator = new Operator(op),
                    RightHandValue = expr
                };
            }
            else
            {
                Parser.RewindTo(previous);
                // rewind and try the next one
            }
            return null;
        }
        // annotated_rhs: yield_expr | star_expressions
        public Expression ParseAnnotatedRhs()
        {
            if (Parser.Peek().Value == Keyword.Yield.Value)
            {
                return Parser.AtomSubParser.ParseYieldExpr();
            }
            else
            {
                return Parser.ParseStarExpressions();
            }
        }
        //augassign:
        //    | '+=' 
        //    | '-=' 
        //    | '*=' 
        //    | '@=' 
        //    | '/=' 
        //    | '%=' 
        //    | '&=' 
        //    | '|=' 
        //    | '^=' 
        //    | '<<=' 
        //    | '>>=' 
        //    | '**=' 
        //    | '//='
        public bool IsAugAssign(string value)
        {
            // TODO @ is for matrix multiplication
            return value == "+=" || value == "-=" || value == "*=" || value == "@="
                || value == "/=" || value == "%=" || value == "&=" || value == "|="
                || value == "^=" || value == "<<=" || value == ">>=" || value == "**="
                || value == "//=";
        }
    }
}
