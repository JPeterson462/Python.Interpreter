using System;
using System.Collections.Generic;
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
        //pattern:
        //    | as_pattern
        //    | or_pattern
        public Pattern ParsePattern()
        {
            OrPattern orPattern = ParseOrPattern();
            if (Parser.Peek().Value == Keyword.As.Value)
            {
                //as_pattern:
                //    | or_pattern 'as' pattern_capture_target
                Parser.Advance();
                orPattern.CaptureTarget = ParsePatternCaptureTarget();
            }
            return orPattern;
        }
        //or_pattern:
        //    | '|'.closed_pattern+
        public OrPattern ParseOrPattern()
        {
            List<Pattern> parts = new List<Pattern>();
            parts.Add(ParseClosedPattern());
            while (Parser.Peek().Value == "|")
            {
                Parser.Advance();
                parts.Add(ParseClosedPattern());
            }
            return new OrPattern
            {
                Parts = parts
            };
        }


        //closed_pattern:
        //    | literal_pattern
        //    | capture_pattern
        //    | wildcard_pattern
        //    | value_pattern
        //    | group_pattern
        //    | sequence_pattern
        //    | mapping_pattern
        //    | class_pattern
        public Pattern ParseClosedPattern()
        {
            if (Parser.Peek().Value == "_")
            {
                // wildcard_pattern
                Parser.Advance();
                return new WildcardPattern();
            }
            if (Parser.Peek().Type == TokenType.Variable &&
                (Parser.Peek(1).Value != "." && Parser.Peek(1).Value != "(" && Parser.Peek(1).Value != "="))
            {
                // capture_pattern
                return ParseCapturePattern();
            }
            if (Parser.Peek().Value == Keyword.True.Value || Parser.Peek().Value == Keyword.False.Value ||
                Parser.Peek().Value == Keyword.None.Value || Parser.Peek().Type == TokenType.String ||
                Parser.Peek().Value == "-" || Parser.Peek().Type == TokenType.Number)
            {
                // literal_pattern
                return ParseLiteralPattern();
            }
            int previous = Parser.Position;
            try
            {
                // value_pattern
                Pattern p = ParseValuePattern();
                if (p != null)
                {
                    return p;
                }
                Parser.RewindTo(previous);
            }
            catch (Exception)
            {
                Parser.RewindTo(previous);
            }
            if (Parser.Peek().Value == "(")
            {
                // group_pattern
                Parser.Advance();
                Pattern p = ParsePattern();
                Parser.Accept(")");
                Parser.Advance();
                return p;
            }
            if (Parser.Peek().Value == "[" || Parser.Peek().Value == "(")
            {
                // sequence_pattern
                return ParseSequencePattern();
            }
            if (Parser.Peek().Value == "{")
            {
                // mapping_pattern
            }
            // class_pattern
            throw new NotImplementedException();
        }
        //sequence_pattern:
        //    | '[' maybe_sequence_pattern? ']' 
        //    | '(' open_sequence_pattern? ')' 
        public Pattern ParseSequencePattern()
        {
            if (Parser.Peek().Value == "[")
            {
                Parser.Advance();
                if(Parser.Peek().Value != "]")
                {
                    SequencePattern p = ParseMaybeSequencePattern();
                    Parser.Accept("]");
                    Parser.Advance();
                    p.Type = SequenceType.List;
                    return p;
                }
                else
                {
                    return new SequencePattern
                    {
                        Type = SequenceType.List,
                        Elements = new List<Pattern>()
                    };
                }
            }
            if (Parser.Peek().Value == "(")
            {
                Parser.Advance();
                if (Parser.Peek().Value != ")")
                {
                    SequencePattern p = ParseOpenSequencePattern();
                    Parser.Accept(")");
                    Parser.Advance();
                    p.Type = SequenceType.Tuple;
                    return p;
                }
                else
                {
                    return new SequencePattern
                    {
                        Type = SequenceType.Tuple,
                        Elements = new List<Pattern>()
                    };
                }
            }
            Parser.ThrowSyntaxError(Parser.Position);
            return null; // shouldn't get here
        }
        //open_sequence_pattern:
        //    | maybe_star_pattern ',' maybe_sequence_pattern?
        public SequencePattern ParseOpenSequencePattern()
        {
            List<Pattern> elements = new List<Pattern>();
            elements.Add(ParseMaybeStarPattern());
            Parser.Accept(",");
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                elements.Add(ParseMaybeStarPattern());
            }
            return new SequencePattern
            {
                Elements = elements
            };
        }
        //maybe_sequence_pattern:
        //    | ','.maybe_star_pattern+ ','?
        public SequencePattern ParseMaybeSequencePattern()
        {
            List<Pattern> elements = new List<Pattern>();
            elements.Add(ParseMaybeStarPattern());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                elements.Add(ParseMaybeStarPattern());
            }
            return new SequencePattern
            {
                Elements = elements
            };
        }
        //maybe_star_pattern:
        //    | star_pattern
        //    | pattern
        public Pattern ParseMaybeStarPattern()
        {
            if (Parser.Peek().Value == "*")
            {
                return ParseStarPattern();
            }
            else
            {
                return ParsePattern();
            }
        }

        //star_pattern:
        //    | '*' pattern_capture_target 
        //    | '*' wildcard_pattern
        public StarPattern ParseStarPattern()
        {
            Parser.Accept("*");
            Parser.Advance();
            if (Parser.Peek().Value == "_")
            {
                Parser.Advance();
                return new StarPattern
                {
                    Pattern = new WildcardPattern()
                };
            }
            else
            {
                return new StarPattern
                {
                    Pattern = ParsePatternCaptureTarget()
                };
            }
        }

        //value_pattern:
        //    | attr !('.' | '(' | '=')
        public Pattern ParseValuePattern()
        {
            AttributePattern attr = ParseAttr();
            if (Parser.Peek().Value != "." && Parser.Peek().Value != "(" && Parser.Peek().Value != "=")
            {
                return attr;
            }
            else
            {
                return null;
            }
        }
        //attr:
        //    | name_or_attr '.' NAME --> '.'NAME+
        public AttributePattern ParseAttr()
        {
            List<string> parts = new List<string>();
            string value = Parser.Peek().Value;
            parts.Add(value);
            Parser.Advance();
            Parser.Accept(".");
            while (Parser.Peek().Value == ".")
            {
                Parser.Advance();
                parts.Add(Parser.Peek().Value);
                Parser.Advance();
            }
            return new AttributePattern
            {
                Parts = parts
            };
        }
        //name_or_attr:
        //    | attr
        //    | NAME
        public Pattern ParseNameOrAttr()
        {
            if (Parser.Peek(1).Value == ".")
            {
                return ParseAttr();
            }
            else
            {
                string value = Parser.Peek().Value;
                Parser.Advance();
                return new AttributePattern
                {
                    Parts = new List<string>(new string[] { value })
                };
            }
        }


        //capture_pattern:
        //    | pattern_capture_target
        public Pattern ParseCapturePattern()
        {
            return ParsePatternCaptureTarget();
        }
        //pattern_capture_target:
        //    | !"_" NAME !('.' | '(' | '=')
        public Pattern ParsePatternCaptureTarget()
        {
            Parser.Accept(TokenType.Variable);
            string name = Parser.Peek().Value;
            Parser.Advance();
            return new VariablePattern
            {
                Variable = name
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
            // try to parse as signed_number
            if (current == "-")
            {
                if (Parser.Peek(2).Value != "+" && Parser.Peek(2).Value != "-")
                {
                    string value = Parser.Peek(1).Value;
                    NumberPattern pattern = new NumberPattern();
                    if (value.EndsWith("j"))
                    {
                        pattern.ImaginaryPart = -1 * double.Parse(value.Substring(0, value.Length - 1));
                    }
                    else
                    {
                        pattern.RealPart = -1 * double.Parse(value.Substring(0, value.Length - 1));
                    }
                    Parser.Advance(2);
                    return pattern;
                }
            }
            else
            {
                if (Parser.Peek(1).Value != "+" && Parser.Peek(1).Value != "-")
                {
                    string value = Parser.Peek().Value;
                    NumberPattern pattern = new NumberPattern();
                    if (value.EndsWith("j"))
                    {
                        pattern.ImaginaryPart = double.Parse(value.Substring(0, value.Length - 1));
                    }
                    else
                    {
                        pattern.RealPart = double.Parse(value.Substring(0, value.Length - 1));
                    }
                    Parser.Advance(1);
                    return pattern;
                }
            }
            return ParseComplexNumber();
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
