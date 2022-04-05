using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class AtomSubParser
    {
        public PythonParser Parser { get; set; }
        public AtomSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //for_if_clauses:
        //    | for_if_clause+
        public List<Expression> ParseForIfClauses()
        {
            List<Expression> clauses = new List<Expression>();
            while (Parser.Peek().Value == Keyword.Async.Value || Parser.Peek().Value == Keyword.For.Value)
            {
                clauses.Add(ParseForIfClause());
            }
            return clauses;
        }
        //for_if_clause:
        //    | ASYNC 'for' star_targets 'in' ~disjunction('if' disjunction )* 
        //    | 'for' star_targets 'in' ~disjunction('if' disjunction )*
        public Expression ParseForIfClause()
        {
            bool isAsync = false;
            if (Parser.Peek().Value == Keyword.Async.Value)
            {
                Parser.Advance();
                isAsync = true;
            }
            Parser.Accept(Keyword.For.Value);
            Parser.Advance();
            Expression target = ParseStarTargets();
            Parser.Accept(Keyword.In.Value);
            Parser.Advance();
            Expression group = Parser.OperationSubParser.ParseDisjunction();
            List<Expression> conditions = new List<Expression>();
            while (Parser.Peek().Value == Keyword.If.Value)
            {
                Parser.Advance();
                conditions.Add(Parser.OperationSubParser.ParseDisjunction());
            }
            return new ForIfGeneratorExpression
            {
                IsAsynchronous = isAsync,
                Targets = target,
                Group = group,
                Conditions = conditions
            };
        }
        public Expression ParseYieldExpr()
        {
            Parser.Accept(Keyword.Yield.Value);
            Parser.Advance();

            if (Parser.Peek().Value == Keyword.From.Value)
            {
                Parser.Accept(Keyword.From.Value);
                Parser.Advance();

                return new YieldExpression
                {
                    CollectionExpression = Parser.ParseExpression()
                };
            }
            else
            {
                return new YieldExpression
                {
                    Expressions = (Parser.ParseStarExpressions() as CollectionExpression).Elements
                };
            }
        }
        // t_primary:
        //   | t_primary '.' NAME &t_lookahead 
        //   | t_primary '[' slices ']' &t_lookahead 
        //   | t_primary genexp &t_lookahead 
        //   | t_primary '(' [arguments] ')' &t_lookahead 
        //   | atom &t_lookahead
        // t_lookahead: '(' | '[' | '.'
        public Expression ParseTPrimary()
        {
            return _ParseTPrimary();
        }
        private Expression _ParseTPrimary()
        {
            Expression atom = ParseAtom();
            Token next = Parser.Peek();
            while (next.Type == TokenType.BeginParameters || next.Type == TokenType.BeginList || next.Type == TokenType.ObjectReference
                || next.Value == "(" || next.Value == "[" || next.Value == ".")
            {
                if (Parser.Peek().Value == "." || Parser.Peek().Type == TokenType.ObjectReference)
                {
                    Parser.Advance();
                    atom = new EvaluatedExpression
                    {
                        LeftHandValue = atom,
                        IsObjectReference = true,
                        RightHandValue = ParseAtom()
                    };
                }
                else if (Parser.Peek().Value == "[" || Parser.Peek().Type == TokenType.BeginList)
                {
                    Parser.Advance();
                    atom = new EvaluatedExpression
                    {
                        LeftHandValue = atom,
                        IsArrayAccessor = true,
                        RightHandValue = ParseSlices()
                    };
                    Parser.Accept(TokenType.EndList);
                    Parser.Advance();
                }
                else if (Parser.Peek().Value == "(" || Parser.Peek().Type == TokenType.BeginParameters)
                {
                    Parser.Advance();
                    atom = new EvaluatedExpression
                    {
                        LeftHandValue = atom,
                        IsFunctionCall = true,
                        RightHandValue = Parser.ArgumentsSubParser.ParseArguments()
                    };
                    Parser.Accept(TokenType.EndParameters);
                    Parser.Advance();
                }
                next = Parser.Peek();
            }
            /*// FIXME genexp
            */
            return atom;
        }
        public Expression ParseAtom()
        {
            Token token = Parser.Peek();
            if (token.Value == Keyword.True.Value || token.Value == Keyword.False.Value ||
                token.Value == Keyword.None.Value || token.Type == TokenType.Number ||
                token.Type == TokenType.String || token.Type == TokenType.Variable)
            {
                bool isBoolean = token.Value == Keyword.True.Value || token.Value == Keyword.False.Value;
                bool isIntegerNumber = token.Type == TokenType.Number && !token.Value.Contains(".");
                bool isFloatingPointNumber = token.Type == TokenType.Number && token.Value.Contains(".");
                bool isString = token.Type == TokenType.String;
                Parser.Advance();
                return new SimpleExpression
                {
                    Value = token.Value,
                    IsConstant = token.Type == TokenType.String || token.Type == TokenType.Number ||
                                token.Value == Keyword.True.Value || token.Value == Keyword.False.Value ||
                                token.Value == Keyword.None.Value,
                    IsVariable = token.Type == TokenType.Variable,
                    ConstantType = isBoolean ? typeof(bool) :
                                        (isString ? typeof(string) :
                                            (isIntegerNumber ? typeof(int) :
                                                (isFloatingPointNumber ? typeof(double) : null)))
                };
            }
            else
            {
                /*
                    list:
                        | '[' [star_named_expressions] ']' 
                    listcomp:
                        | '[' named_expression for_if_clauses ']' 
                    tuple:
                        | '(' [star_named_expression ',' [star_named_expressions]  ] ')' 
                    group:
                        | '(' (yield_expr | named_expression) ')' 
                    genexp:
                        | '(' ( assignment_expression | expression !':=') for_if_clauses ')' 
                    set: '{' star_named_expressions '}' 
                    setcomp:
                        | '{' named_expression for_if_clauses '}' 
                    dict:
                        | '{' [double_starred_kvpairs] '}' 

                    dictcomp:
                        | '{' kvpair for_if_clauses '}' 
                 */
                if(token.Type == TokenType.BeginParameters) // (
                {
                    Parser.Advance();
                    Expression element = null;
                    Parser.RewindTo(Parser.Position - 1);
                    int position = Parser.Position;
                    try
                    {
                        Expression generator = ParseGenexp();
                        return generator;
                    }
                    catch (Exception ex)
                    {
                        Parser.RewindTo(position + 1);
                    }
                    if (Parser.Peek().Value == Keyword.Yield.Value)
                    {
                        element = ParseYieldExpr();

                        Parser.Accept(TokenType.EndParameters);
                        Parser.Advance();

                        return new CollectionExpression
                        {
                            Elements = new List<Expression>(new Expression[] { element }),
                            Type = CollectionType.GeneratedTuple
                        };
                    }
                    else
                    {
                        // TODO handling group that's not a star_named_expression, is this correct?
                        element = Parser.ParseStarNamedExpression();
                    }
                    List<Expression> elements = new List<Expression>();
                    elements.Add(element);
                    while (Parser.Peek().Value == ",")
                    {
                        Parser.Advance();
                        elements.Add(Parser.ParseStarNamedExpression());
                    }
                    element = new CollectionExpression
                    {
                        Elements = elements,
                        Type = CollectionType.Tuple
                    };

                    Parser.Accept(TokenType.EndParameters);
                    Parser.Advance();

                    return element;
                }
                else if (token.Type == TokenType.BeginList) // [
                {
                    Parser.Advance();
                    CollectionExpression collection = new CollectionExpression
                    {
                        Elements = new List<Expression>()
                    };
                    Expression element = Parser.ParseStarNamedExpression();
                    if (Parser.Peek().Value == Keyword.Async.Value || Parser.Peek().Value == Keyword.For.Value)
                    {
                        List<Expression> clauses = ParseForIfClauses();
                        return new GeneratorExpression
                        {
                            Target = element,
                            Generator = new CollectionExpression
                            {
                                Elements = clauses
                            }
                        };
                    }
                    else
                    {
                        collection.Type = CollectionType.List;
                        collection.Elements.Add(element);
                        while (Parser.Peek().Type != TokenType.EndList)
                        {
                            Parser.Accept(TokenType.ElementSeparator);
                            Parser.Advance();
                            collection.Elements.Add(Parser.ParseStarNamedExpression());
                        }
                    }
                    Parser.Accept(TokenType.EndList);
                    Parser.Advance();
                    return collection;
                }
                else if (token.Type == TokenType.DictionaryStart) // {
                {
                    Parser.Advance();

                    // FIXME implement

                    Parser.Accept(TokenType.DictionaryEnd);
                    Parser.Advance();
                }
                else
                {
                    Parser.ThrowSyntaxError(Parser.Position);
                }
            }


            throw new NotImplementedException();
        }
        //star_targets:
        //| star_target !',' 
        //| star_target(',' star_target )* [',']
        public CollectionExpression ParseStarTargets()
        {
            CollectionExpression collection = new CollectionExpression
            {
                Type = CollectionType.List
            };
            collection.Elements.Add(ParseStarTarget());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                collection.Elements.Add(ParseStarTarget());
            }
            return collection;
        }
        //star_targets_list_seq: ','.star_target+ [',']
        public CollectionExpression ParseStarTargetsListSeq()
        {
            CollectionExpression collection = new CollectionExpression
            {
                Type = CollectionType.List
            };
            collection.Elements.Add(ParseStarTarget());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                collection.Elements.Add(ParseStarTarget());
            }
            return collection;
        }
        //star_targets_tuple_seq:
        //| star_target(',' star_target )+ [',']
        //| star_target ','
        public CollectionExpression ParseStarTargetsTupleSeq()
        {
            CollectionExpression collection = new CollectionExpression
            {
                Type = CollectionType.List
            };
            collection.Elements.Add(ParseStarTarget());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                collection.Elements.Add(ParseStarTarget());
            }
            return collection;
        }
        //star_target:
        //| '*' (!'*' star_target) 
        //| target_with_star_atom
        public Expression ParseStarTarget()
        {
            if (Parser.Peek().Value == "*")
            {
                Parser.Advance();
                Parser.DontAccept("*");
                return ParseStarTarget();
            }
            else
            {
                return ParseTargetWithStarAtom();
            }
        }
        //target_with_star_atom:
        //| t_primary '.' NAME !t_lookahead
        //| t_primary '[' slices ']' !t_lookahead
        //| star_atom
        public Expression ParseTargetWithStarAtom()
        {
            int position = Parser.Position;
            try
            {
                Expression tprimary = ParseTPrimary();
                if (Parser.Peek().Value == ".")
                {
                    Parser.Advance();
                    string name = Parser.OperatorSubParser.ParseName();
                    DontAcceptTLookahead();
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
                else if (Parser.Peek().Value == "[")
                {
                    Parser.Advance();
                    Expression slices = Parser.AtomSubParser.ParseSlices();
                    Parser.Accept("]");
                    Parser.Advance();
                    DontAcceptTLookahead();
                    return new EvaluatedExpression
                    {
                        LeftHandValue = tprimary,
                        RightHandValue = slices,
                        IsArrayAccessor = true
                    };
                }
                else
                {
                    Parser.ThrowSyntaxError(Parser.Position);
                }
            }
            catch (Exception)
            {
                Parser.RewindTo(position);
                return ParseStarAtom();
            }
            return null; // shouldn't get here
        }
        //star_atom:
        //| NAME
        //| '(' target_with_star_atom ')' 
        //| '(' [star_targets_tuple_seq] ')' 
        //| '[' [star_targets_list_seq] ']'
        public Expression ParseStarAtom()
        {
            if (Parser.Peek().Value == "(")
            {
                Parser.Advance();
                Expression ex = ParseStarTargetsTupleSeq();
                Parser.Accept(")");
                Parser.Advance();
                return ex;
            }
            if (Parser.Peek().Value == "[")
            {
                Parser.Advance();
                Expression ex = ParseStarTargetsListSeq();
                Parser.Accept("]");
                Parser.Advance();
                return ex;
            }
            Parser.Accept(TokenType.Variable);
            string value = Parser.Peek().Value;
            Parser.Advance();
            return new SimpleExpression
            {
                Value = value,
                IsVariable = true
            };
        }
        //single_target:
        //    | single_subscript_attribute_target
        //    | NAME 
        //    | '(' single_target ')'
        public Expression ParseSingleTarget()
        {
            Expression expression = ParseSingleSubscriptAttributeTarget();
            if (expression == null)
            {
                if (Parser.Peek().Type == TokenType.BeginParameters || Parser.Peek().Value == "(")
                {
                    expression = ParseSingleTarget();
                    Parser.Accept(TokenType.EndParameters);
                    Parser.Advance();
                }
                else
                {
                    expression = new SimpleExpression
                    {
                        IsVariable = true,
                        IsConstant = false,
                        Value = Parser.OperatorSubParser.ParseName()
                    };
                }
            }
            return expression;
        }
        //single_subscript_attribute_target:
        //    | t_primary '.' NAME !t_lookahead 
        //    | t_primary '[' slices ']' !t_lookahead
        public Expression ParseSingleSubscriptAttributeTarget()
        {
            int position = Parser.Position;
            Expression primary = ParseTPrimary();
            if (Parser.Peek().Type == TokenType.ObjectReference || Parser.Peek().Value == ".")
            {
                Parser.Advance();
                string name = Parser.OperatorSubParser.ParseName();
                DontAcceptTLookahead();
                return new EvaluatedExpression
                {
                    LeftHandValue = primary,
                    KeywordOperator = null,
                    Operator = null,
                    IsObjectReference = true,
                    RightHandValue = new SimpleExpression
                    {
                        IsVariable = true,
                        IsConstant = false,
                        Value = name
                    }
                };
            }
            else if (Parser.Peek().Type == TokenType.BeginList || Parser.Peek().Value == "[")
            {
                Parser.Advance();
                Expression slices = ParseSlices();
                DontAcceptTLookahead();
                return new EvaluatedExpression
                {
                    LeftHandValue = primary,
                    KeywordOperator = null,
                    Operator = null,
                    IsArrayAccessor = true,
                    RightHandValue = slices
                };
            }
            else
            {
                Parser.RewindTo(position);
                return null;
            }
        }
        // t_lookahead: '(' | '[' | '.'
        public void DontAcceptTLookahead()
        {
            Token next = Parser.Peek();
            if (next.Type == TokenType.BeginParameters || next.Type == TokenType.BeginList || next.Type == TokenType.ObjectReference
                || next.Value == "(" || next.Value == "[" || next.Value == ".")
            {
                Parser.ThrowSyntaxError(Parser.Position);
            }
        }
        //    slices:
        //    | slice !',' 
        //    | ','.slice+ [',']
        //    slice:
        //    | [expression] ':' [expression] [':' [expression] ] 
        //    | named_expression
        public Expression ParseSlices()
        {
            CollectionExpression slices = new CollectionExpression
            {
                Type = CollectionType.Slices
            };
            Expression slice = ParseSlice();
            slices.Elements.Add(slice);
            while (Parser.Peek().Type == TokenType.ElementSeparator || Parser.Peek().Value == ",")
            {
                Parser.Advance();
                slice = ParseSlice();
                slices.Elements.Add(slice);
            }
            return slices;
        }
        public Expression ParseSlice()
        {
            int position = Parser.Position;
            Expression start = IsColonNext() ? null : Parser.ParseExpression();
            if (IsColonNext())
            {
                Parser.Advance();
                Expression stop = IsColonNext() ? null : Parser.ParseExpression();
                if (IsColonNext())
                {
                    Parser.Advance();
                    Expression interval = Parser.ParseExpression();
                    return new SliceExpression
                    {
                        Start = start,
                        Stop = stop,
                        Interval = interval
                    };
                }
                else
                {
                    return new SliceExpression
                    {
                        Start = start,
                        Stop = stop,
                        Interval = null
                    };
                }
            }
            else
            {
                Parser.RewindTo(position);
                Expression elem = Parser.ParseNamedExpression();
                return new SliceExpression
                {
                    Start = elem,
                    Stop = elem,
                    IsExpression = true,
                    Interval = new SimpleExpression
                    {
                        IsConstant = true,
                        Value = "0"
                    }
                };
            }
        }
        public bool IsColonNext()
        {
            return Parser.Peek().Type == TokenType.BeginBlock || Parser.Peek().Value == ":";
        }

        // genexp:
        //  | '(' (assignment_expression | expression !':=') for_if_clauses ')' 
        public Expression ParseGenexp()
        {
            Parser.Accept("(");
            Parser.Advance();
            Expression target = null;
            if (Parser.Peek(1).Value == ":=")
            {
                target = Parser.ParseAssignmentExpression();
            }
            else
            {
                target = Parser.ParseExpression();
            }
            List<Expression> clauses = ParseForIfClauses();
            Parser.Accept(")");
            Parser.Advance();
            return new GeneratorExpression
            {
                Generator = new CollectionExpression
                {
                    Elements = clauses,
                    Type = CollectionType.Generator
                },
                Target = target
            };
        }
        // set: '{' star_named_expressions '}'
        public CollectionExpression ParseSet()
        {
            Parser.Accept("{");
            Parser.Advance();
            List<Expression> elements = new List<Expression>();
            elements.Add(Parser.ParseStarNamedExpression());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                elements.Add(Parser.ParseStarNamedExpression());
            }
            Parser.Accept("}");
            Parser.Advance();
            return new CollectionExpression
            {
                Elements = elements,
                Type = CollectionType.Set
            };
        }

        // setcomp:
        //    | '{' named_expression for_if_clauses '}'
        public CollectionExpression ParseSetcomp()
        {
            Parser.Accept("{");
            Parser.Advance();
            Expression target = Parser.ParseNamedExpression();
            var generator = ParseForIfClauses();
            GeneratorExpression expr = new GeneratorExpression
            {
                Generator = new CollectionExpression
                {
                    Elements = generator,
                    Type = CollectionType.List
                },
                Target = target
            };
            Parser.Accept("}");
            Parser.Advance();
            return new CollectionExpression
            {
                Elements = new List<Expression>(new Expression[] { expr }),
                Type = CollectionType.Dictionary
            };
        }

        // dict:
        //    | '{' [double_starred_kvpairs] '}'
        public CollectionExpression ParseDict()
        {
            Parser.Accept("{");
            Parser.Advance();
            var pairs = ParseDoubleStarredKvpairs();
            Parser.Accept("}");
            Parser.Advance();
            return new CollectionExpression
            {
                Elements = pairs,
                Type = CollectionType.Dictionary
            };
        }
        //
        // dictcomp:
        //    | '{' kvpair for_if_clauses '}'
        public CollectionExpression ParseDictcomp()
        {
            Parser.Accept("{");
            Parser.Advance();
            Expression target = ParseKvpair();
            var generator = ParseForIfClauses();
            GeneratorExpression expr = new GeneratorExpression
            {
                Generator = new CollectionExpression
                {
                    Elements = generator,
                    Type = CollectionType.List
                },
                Target = target
            };
            Parser.Accept("}");
            Parser.Advance();
            return new CollectionExpression
            {
                Elements = new List<Expression>(new Expression[] { expr }),
                Type = CollectionType.Dictionary
            };
        }

        //double_starred_kvpairs: ','.double_starred_kvpair+ [',']
        public List<Expression> ParseDoubleStarredKvpairs()
        {
            List<Expression> elements = new List<Expression>();
            elements.Add(ParseDoubleStarredKvpair());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                elements.Add(ParseDoubleStarredKvpair());
            }
            return elements;
        }
        //double_starred_kvpair:
        //    | '**' bitwise_or 
        //    | kvpair
        public Expression ParseDoubleStarredKvpair()
        {
            if (Parser.Peek().Value == "**")
            {
                Parser.Advance();
                Expression bitwiseOr = Parser.OperationSubParser.ParseBitwiseOr();
                return new OperatorExpression
                {
                    Operator = Operator.Exponentiation,
                    Expression = bitwiseOr
                };
            }
            else
            {
                return ParseKvpair();
            }
        }
        //kvpair: expression ':' expression
        public KeyValueExpression ParseKvpair()
        {
            Expression key = Parser.ParseExpression();
            Parser.Accept(":");
            Parser.Advance();
            Expression value = Parser.ParseExpression();
            return new KeyValueExpression
            {
                Key = key,
                Value = value
            };
        }
    }
}
