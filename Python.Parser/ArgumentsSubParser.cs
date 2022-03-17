using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class ArgumentsSubParser
    {
        public PythonParser Parser { get; set; }
        public ArgumentsSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //arguments:
        //    | args[','] &')'
        public Expression ParseArguments()
        {
            Expression args = ParseArgs();
            Parser.Accept(")");
            return args;
        }
        //args:
        //    | ','.(starred_expression | (assignment_expression | expression !':=') !'=')+ [',' kwargs] 
        //    | kwargs
        //
        private bool NotKwargs()
        {
            return
                Parser.Peek().Value == "*" || // starred_expression
                Parser.Peek(1).Value != "=" // (assignment_expression | expression !':=') !'='
                ;
        }
        public Expression ParseArgs()
        {
            List<Expression> arguments = new List<Expression>();
            // ','.(starred_expression | ( assignment_expression | expression !':=') !'=')+
            if (NotKwargs())
            {
                if (Parser.Peek().Value == "*")
                {
                    arguments.Add(ParseStarredExpression());
                }
                else
                {
                    if (Parser.Peek(1).Value == ":=")
                    {
                        arguments.Add(Parser.ParseAssignmentExpression());
                    }
                    else
                    {
                        arguments.Add(Parser.ParseExpression());
                    }
                }
                while (NotKwargs() && Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    if (Parser.Peek().Value == "*")
                    {
                        arguments.Add(ParseStarredExpression());
                    }
                    else
                    {
                        if (Parser.Peek(1).Value == ":=")
                        {
                            arguments.Add(Parser.ParseAssignmentExpression());
                        }
                        else
                        {
                            arguments.Add(Parser.ParseExpression());
                        }
                    }
                }
            }
            arguments.AddRange(((CollectionExpression)ParseKwargs()).Elements);
            return new CollectionExpression
            {
                Elements = arguments,
                Type = CollectionType.Unknown
            };
        }
        //kwargs:
        //    | ','.kwarg_or_starred+ ',' ','.kwarg_or_double_starred+ 
        //    | ','.kwarg_or_starred+
        //    | ','.kwarg_or_double_starred+
        public Expression ParseKwargs()
        {
            List<Expression> arguments = new List<Expression>();
            if (Parser.Peek().Value == Operator.Exponentiation.Value || Parser.Peek().Value == "**")
            {
                arguments.Add(ParseKwargOrDoubleStarred());
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    arguments.Add(ParseKwargOrDoubleStarred());
                }
            }
            else
            {
                arguments.Add(ParseKwargOrStarred());
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    if (Parser.Peek().Value == "**" || Parser.Peek().Value == Operator.Exponentiation.Value)
                    {
                        // move onto kwarg_or_double_starred
                    }
                    else
                    {
                        arguments.Add(ParseKwargOrStarred());
                    }
                }
                while (Parser.Peek().Value == ",")
                {
                    while (Parser.Peek().Value == ",")
                    {
                        Parser.Advance();
                        arguments.Add(ParseKwargOrDoubleStarred());
                    }
                }
            }
            return new CollectionExpression
            {
                Elements = arguments,
                Type = CollectionType.Unknown
            };
        }
        //starred_expression:
        //    | '*' expression
        public Expression ParseStarredExpression()
        {
            Parser.Accept("*");
            Parser.Advance();
            return new ArgumentExpression
            {
                Expression = Parser.ParseExpression(),
                UnpackIterable = true
            };
        }
        //kwarg_or_starred:
        //    | NAME '=' expression 
        //    | starred_expression
        public Expression ParseKwargOrStarred()
        {
            if (Parser.Peek().Value == Operator.Multiply.Value || Parser.Peek().Value == "*")
            {
                return ParseStarredExpression();
            }
            else
            {
                string name = Parser.Peek().Value;
                Parser.Advance();
                Parser.Accept("=");
                Parser.Advance();
                return new ArgumentExpression
                {
                    Name = name,
                    UnpackDictionary = false,
                    Expression = Parser.ParseExpression()
                };
            }
        }
        //kwarg_or_double_starred:
        //    | NAME '=' expression 
        //    | '**' expression
        public Expression ParseKwargOrDoubleStarred()
        {
            if (Parser.Peek().Value == Operator.Exponentiation.Value || Parser.Peek().Value == "**")
            {
                Parser.Advance();
                return new ArgumentExpression
                {
                    UnpackDictionary = true,
                    Expression = Parser.ParseExpression()
                };
            }
            else
            {
                string name = Parser.Peek().Value;
                Parser.Advance();
                Parser.Accept("=");
                Parser.Advance();
                return new ArgumentExpression
                {
                    Name = name,
                    UnpackDictionary = false,
                    Expression = Parser.ParseExpression()
                };
            }
        }
    }
}
