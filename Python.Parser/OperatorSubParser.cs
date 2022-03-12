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
            // FIXME TODO
            //| star_expressions
            //assignment

            throw new NotImplementedException();
        }
        // del_targets: ','.del_target+ [','] 
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
            // FIXME
            throw new NotImplementedException();
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
    }
}
