﻿using System;
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
        public Expression ParseForIfClause()
        {
            bool isAsync = false;
            if (Parser.Peek().Value == Keyword.Async.Value)
            {
                Parser.Advance();
                isAsync = true;
            }
            Parser.Accept(Keyword.For.Value);

            throw new NotImplementedException();
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
                    if (Parser.Peek().Value == Keyword.Yield.Value)
                    {
                        element = ParseYieldExpr();

                        Parser.Accept(TokenType.EndParameters);
                        Parser.Advance();

                        return element;
                    }
                    else
                    {
                        // FIXME handling group that's not a star_named_expression, is this correct?
                        element = Parser.ParseStarNamedExpression();
                    }
                    List<Expression> elements = new List<Expression>();
                    elements.Add(element);
                    while (Parser.Peek().Value == ",")
                    {
                        Parser.Advance();

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
                        // FIXME implement
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
            throw new NotImplementedException();
        }
    }
}
