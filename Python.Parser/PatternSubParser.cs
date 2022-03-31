using System;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class PatternSubParser
    {
        public PythonParser Parser { get; set; }
        public PatternSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //patterns:
        //    | open_sequence_pattern 
        //    | pattern
        public PatternExpression ParsePatterns()
        {
            // FIXME
            return new PatternExpression
            {
                Pattern = ParseLiteralPattern()
            };
        }

        //literal_pattern:
        //    | signed_number !('+' | '-')
        //    | complex_number
        //    | strings
        //    | 'None' 
        //    | 'True' 
        //    | 'False'
        public Pattern ParseLiteralPattern()
        {
            string current = Parser.Peek().Value;
            TokenType type = Parser.Peek().Type;
            if (type == TokenType.Keyword && current == Keyword.True.Value)
            {
                Parser.Advance();
                return new BooleanPattern
                {
                    Value = true
                };
            }
            if (type == TokenType.Keyword && current == Keyword.False.Value)
            {
                Parser.Advance();
                return new BooleanPattern
                {
                    Value = false
                };
            }
            if (type == TokenType.Keyword && current == Keyword.None.Value)
            {
                Parser.Advance();
                return new NonePattern();
            }
            if (type == TokenType.String)
            {
                Parser.Advance();
                return new StringPattern
                {
                    Value = current
                };
            }
            return ParseComplexNumber(); // should handle both signed_number and complex_number
        }
        //literal_expr:
        //    | signed_number !('+' | '-')
        //    | complex_number
        //    | strings
        //    | 'None' 
        //    | 'True' 
        //    | 'False'
        public Pattern ParseLiteralExpr()
        {
            string current = Parser.Peek().Value;
            TokenType type = Parser.Peek().Type;
            if (type == TokenType.Keyword && current == Keyword.True.Value)
            {
                Parser.Advance();
                return new BooleanPattern
                {
                    Value = true
                };
            }
            if (type == TokenType.Keyword && current == Keyword.False.Value)
            {
                Parser.Advance();
                return new BooleanPattern
                {
                    Value = false
                };
            }
            if (type == TokenType.Keyword && current == Keyword.None.Value)
            {
                Parser.Advance();
                return new NonePattern();
            }
            if (type == TokenType.String)
            {
                Parser.Advance();
                return new StringPattern
                {
                    Value = current
                };
            }
            return ParseComplexNumber(); // should handle both signed_number and complex_number
        }
        //complex_number:
        //    | signed_real_number '+' imaginary_number 
        //    | signed_real_number '-' imaginary_number
        public NumberPattern ParseComplexNumber()
        {
            NumberPattern realPart = ParseSignedRealNumber();
            int imaginarySign = 1;
            if (Parser.Peek().Value == "+")
            {
                imaginarySign = 1;
                Parser.Advance();
            }
            else if (Parser.Peek().Value == "-")
            {
                imaginarySign = -1;
                Parser.Advance();
            }
            // TODO throw syntax error? is the sign always going to be it's own token?
            NumberPattern imaginaryPart = ParseImaginaryNumber();
            return new NumberPattern
            {
                RealPart = realPart.RealPart,
                ImaginaryPart = imaginaryPart.ImaginaryPart * imaginarySign
            };
        }
        //signed_number:
        //    | NUMBER
        //    | '-' NUMBER
        public NumberPattern ParseSignedNumber()
        {
            int sign = 1;
            if (Parser.Peek().Value == "-")
            {
                sign = -1;
                Parser.Advance();
            }
            Parser.Accept(TokenType.Number);
            Token token = Parser.Peek();
            Parser.Advance();
            return new NumberPattern
            {
                RealPart = double.Parse(token.Value) * sign
            };
        }
        //signed_real_number:
        //   | real_number
        //   | '-' real_number
        public NumberPattern ParseSignedRealNumber()
        {
            int sign = 1;
            if (Parser.Peek().Value == "-")
            {
                sign = -1;
                Parser.Advance();
            }
            NumberPattern p = ParseRealNumber();
            p.RealPart *= sign;
            return p;
        }
        //real_number:
        //   | NUMBER
        public NumberPattern ParseRealNumber()
        {
            Parser.Accept(TokenType.Number);
            Token token = Parser.Peek();
            Parser.Advance();
            return new NumberPattern
            {
                RealPart = double.Parse(token.Value)
            };
        }
        //imaginary_number:
        //   | NUMBER
        public NumberPattern ParseImaginaryNumber()
        {
            Parser.Accept(TokenType.Number);
            Token token = Parser.Peek();
            Parser.Advance();
            return new NumberPattern
            {
                ImaginaryPart = double.Parse(token.Value.Substring(0, token.Value.Length - 1)) // ignore the j
            };
        }
    }
}
