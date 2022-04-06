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
            int previous = Parser.Position;
            //try
            {
                Pattern p = ParseOpenSequencePattern();
                if (p != null && !Parser.HasErrors())
                {
                    return new PatternExpression
                    {
                        Pattern = p
                    };
                }
                Parser.RewindTo(previous);
            }
            //catch (Exception)
            {
                Parser.RewindTo(previous);
            }
            return new PatternExpression
            {
                Pattern = ParsePattern()
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
            //try
            {
                if (Parser.Peek().Type == TokenType.Variable)
                {
                    // value_pattern
                    Pattern p = ParseValuePattern();
                    if (p != null && !Parser.HasErrors())
                    {
                        return p;
                    }
                    Parser.RewindTo(previous);
                }
            }
            //catch (Exception)
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
                return ParseMappingPattern();
            }
            // class_pattern
            return ParseClassPattern();
        }
        //class_pattern:
        //    | name_or_attr '(' ')' 
        //    | name_or_attr '(' positional_patterns ','? ')' 
        //    | name_or_attr '(' keyword_patterns ','? ')' 
        //    | name_or_attr '(' positional_patterns ',' keyword_patterns ','? ')'
        public ClassPattern ParseClassPattern()
        {
            AttributePattern name = ParseNameOrAttr() as AttributePattern;
            Parser.Accept("(");
            Parser.Advance();
            List<Pattern> parameters = new List<Pattern>();
            if (Parser.Peek(1).Value != "=")
            {
                parameters.AddRange(ParsePositionalPatterns());
                if (Parser.Peek().Value == "," && Parser.Peek(1).Value != ")")
                {
                    Parser.Advance();
                    parameters.AddRange(ParseKeywordPatterns());
                }
            }
            else
            {
                parameters.AddRange(ParseKeywordPatterns());
            }
            bool open = false;
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                open = true;
            }
            Parser.Accept(")");
            Parser.Advance();
            return new ClassPattern
            {
                Name = name,
                IsOpen = open,
                Values = parameters
            };
        }
        //positional_patterns:
        //    | ','.pattern+
        public List<Pattern> ParsePositionalPatterns()
        {
            List<Pattern> items = new List<Pattern>();
            items.Add(ParsePattern());
            while (Parser.Peek().Value == "," && Parser.Peek(2).Value != "=" && Parser.Peek(1).Value != ")")
            {
                Parser.Advance();
                items.Add(ParsePattern());
            }
            return items;
        }
        //keyword_patterns:
        //    | ','.keyword_pattern+
        public List<Pattern> ParseKeywordPatterns()
        {
            List<Pattern> items = new List<Pattern>();
            items.Add(ParseKeywordPattern());
            while (Parser.Peek().Value == "," && Parser.Peek(1).Value != ")")
            {
                Parser.Advance();
                items.Add(ParseKeywordPattern());
            }
            return items;
        }
        //keyword_pattern:
        //    | NAME '=' pattern
        public KeywordPattern ParseKeywordPattern()
        {
            string name = Parser.Peek().Value;
            Parser.Advance();
            Parser.Accept("=");
            Parser.Advance();
            return new KeywordPattern
            {
                Name = name,
                Value = ParsePattern()
            };
        }
        // mapping_pattern:
        //    | '{' '}' 
        //    | '{' double_star_pattern ','? '}' 
        //    | '{' items_pattern ',' double_star_pattern ','? '}' 
        //    | '{' items_pattern ','? '}'
        public DictionaryPattern ParseMappingPattern()
        {
            Parser.Accept("{");
            Parser.Advance();
            DictionaryPattern pattern = new DictionaryPattern();
            if (Parser.Peek().Value == "}")
            {
                Parser.Advance();
                return pattern;
            }
            else if (Parser.Peek().Value == "**")
            {
                Pattern p = ParseDoubleStarPattern();
                bool open = false;
                if (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    open = true;
                }
                Parser.Accept("}");
                Parser.Advance();
                pattern.ExpandedEntry = p;
                pattern.IsOpen = open;
                return pattern;
            }
            else
            {
                List<Tuple<Pattern, Pattern>> items = ParseItemsPattern();
                pattern.Entries = new Dictionary<Pattern, Pattern>();
                foreach (var item in items)
                {
                    pattern.Entries.Add(item.Item1, item.Item2);
                }
                if (Parser.Peek(1).Value == "**")
                {
                    Parser.Accept(",");
                    Parser.Advance();
                    pattern.ExpandedEntry = ParseDoubleStarPattern();
                    bool open = false;
                    if (Parser.Peek().Value == ",")
                    {
                        Parser.Advance();
                        open = true;
                    }
                    Parser.Accept("}");
                    Parser.Advance();
                    pattern.IsOpen = open;
                    return pattern;
                }
                else
                {
                    bool open = false;
                    if (Parser.Peek().Value == ",")
                    {
                        Parser.Advance();
                        open = true;
                    }
                    Parser.Accept("}");
                    Parser.Advance();
                    pattern.IsOpen = open;
                    return pattern;
                }
            }
        }
        // items_pattern:
        //    | ','.key_value_pattern+
        public List<Tuple<Pattern, Pattern>> ParseItemsPattern()
        {
            List<Tuple<Pattern, Pattern>> items = new List<Tuple<Pattern, Pattern>>();
            items.Add(ParseKeyValuePattern());
            while (Parser.Peek().Value == "," && Parser.Peek(1).Value != "}")
            {
                Parser.Advance();
                items.Add(ParseKeyValuePattern());
            }
            return items;
        }
        // key_value_pattern:
        //    | (literal_expr | attr) ':' pattern
        public Tuple<Pattern, Pattern> ParseKeyValuePattern()
        {
            Pattern key = null;
            if (Parser.Peek().Value == Keyword.True.Value || Parser.Peek().Value == Keyword.False.Value ||
                Parser.Peek().Value == Keyword.None.Value || Parser.Peek().Type == TokenType.String ||
                Parser.Peek().Value == "-" || Parser.Peek().Type == TokenType.Number)
            {
                // literal_pattern
                key = ParseLiteralPattern();
            }
            else
            {
                key = ParseAttr();
            }
            Parser.Accept(TokenType.BeginBlock);
            Parser.Advance();
            Pattern value = ParsePattern();
            return new Tuple<Pattern, Pattern>(key, value);
        }
        // double_star_pattern:
        //   | '**' pattern_capture_target
        public Pattern ParseDoubleStarPattern()
        {
            Parser.Accept("**");
            Parser.Advance();
            return ParsePatternCaptureTarget();
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
                Elements = elements,
                IsOpen = elements.Count == 1
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
                        pattern.RealPart = -1 * double.Parse(value.Substring(0, value.Length));
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
                        pattern.RealPart = double.Parse(value.Substring(0, value.Length));
                    }
                    Parser.Advance(1);
                    return pattern;
                }
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
                        pattern.RealPart = -1 * double.Parse(value.Substring(0, value.Length));
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
                        pattern.RealPart = double.Parse(value.Substring(0, value.Length));
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
