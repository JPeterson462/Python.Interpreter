using System;
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
        //args:
        //    | ','.(starred_expression | (assignment_expression | expression !':=') !'=')+ [',' kwargs] 
        //    | kwargs
        //
        //kwargs:
        //    | ','.kwarg_or_starred+ ',' ','.kwarg_or_double_starred+ 
        //    | ','.kwarg_or_starred+
        //    | ','.kwarg_or_double_starred+
        public Expression ParseKwargs()
        {
            throw new NotImplementedException();
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
