using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;

namespace Python.Tokenizer
{
    public class PythonTokenizer : Tokenizer
    {
        private static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string Numbers = "0123456789";

        private IEnumerable<Keyword> longestKeywords;
        private IEnumerable<Operator> longestOperators;
        private List<char> allowedVariableCharacters = new List<char>((UppercaseLetters + LowercaseLetters + Numbers + "_").ToCharArray());
        private List<char> allowedVariablePrefixes = new List<char>((UppercaseLetters + LowercaseLetters + Numbers + "_").ToCharArray());
        // variables shouldn't start with _ but numpy has __all__?
        private List<char> allowedNumberCharacters = new List<char>((Numbers + "e.j").ToCharArray());
        private int previousTabDistance = 0;
        public PythonTokenizer(string source) : base(source)
        {
            longestKeywords = Keyword.ALL.ToList().OrderByDescending(kw => kw.Value.Length);
            longestOperators = Operator.ALL.ToList().OrderByDescending(op => op.Value.Length);
        }
        public override Token NextToken()
        {
            if (GetCurrentCharacter() == '\\' && Source[Position + 1] == '\n')
            {
                // \ is a non-breaking newline
                SkipNext(2);
                return null;
            }
            if (Position > 0 && Source[Position - 1] == '\n' && previousTabDistance > 0
                && !(Source[Position] == '\t' || Source[Position] == ' '))
            {
                // if we've started a new line and the last line had an indent but this doesn't, add a Dedent
                previousTabDistance = 0;
                return new Token
                {
                    Type = TokenType.DedentTab,
                    Value = null,
                    Count = 0
                };
            }
            if (GetCurrentCharacter() == '\t' || GetCurrentCharacter() == ' ')
            {
                //Advance();
                int start = Position, end = start + 1;
                while (Source[end] == '\t' || Source[end] == ' ')
                {
                    end++;
                }
                SkipNext(end - start);
                int distance = end - start, previousDistance = previousTabDistance;
                previousTabDistance = distance;
                return new Token
                {
                    Type = distance == previousDistance ? TokenType.Tab : (distance > previousDistance ? TokenType.IndentTab : TokenType.DedentTab),
                    Value = null,
                    Count = distance
                };
            }
            if (GetCurrentCharacter() == '\n' || GetCurrentCharacter() == ';')
            {
                string value = string.Empty + GetCurrentCharacter();
                Advance();
                return new Token
                {
                    Type = TokenType.EndOfExpression,
                    Value = value
                };
            }
            if (GetCurrentCharacter() == '#')
            {
                // single line comment
                Advance();
                int start = Position, end = start + 1;
                while (Source[end] != '\n')
                {
                    end++;
                }
                SkipNext(end - start);
                return new Token
                {
                    Type = TokenType.Comment,
                    Value = Source.Substring(start, end - start).Trim()
                };
            }
            if (GetNext(2) == "->")
            {
                SkipNext(2);
                return new Token
                {
                    Type = TokenType.ReturnHint,
                    Value = null
                };
            }
            if (GetNext(3) == "\"\"\"")
            {
                // multi line comment
                SkipNext(3);
                int start = Position, end = start + 1;
                while (Source.Length - end >= 3 && Source.Substring(end, 3) != "\"\"\"")
                {
                    end++;
                }
                SkipNext(end - start);
                SkipNext(3);
                return new Token
                {
                    Type = TokenType.Comment,
                    Value = Source.Substring(start, end - start).Trim()
                };
            }
            if (GetCurrentCharacter() == '(')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.BeginParameters,
                    Value = "("
                };
            }
            if (GetCurrentCharacter() == ')')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.EndParameters,
                    Value = ")"
                };
            }
            if (GetCurrentCharacter() == '[')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.BeginList,
                    Value = "["
                };
            }
            if (GetCurrentCharacter() == ']')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.EndList,
                    Value = "]"
                };
            }
            if (GetCurrentCharacter() == '{')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.DictionaryStart,
                    Value = "{"
                };
            }
            if (GetCurrentCharacter() == '}')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.DictionaryEnd,
                    Value = "}"
                };
            }
            if (GetCurrentCharacter() == ',')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.ElementSeparator,
                    Value = ","
                };
            }
            if (GetCurrentCharacter() == '@')
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.Decorator,
                    Value = "@"
                };
            }
            if (GetCurrentCharacter() == '.' && PreviousToken?.Type == TokenType.Variable)
            {
                Advance();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.ObjectReference,
                    Value = "."
                };
            }
            Keyword kw = NextKeyword();
            if (kw != null)
            {
                SkipNext(kw.Length);
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.Keyword,
                    Value = kw.Value
                };
            }
            Operator op = NextOperator();
            if (op != null)
            {
                SkipNext(op.Length);
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.Operator,
                    Value = op.Value
                };
            }
            char next = GetCurrentCharacter();
            if (next == '\'' || next == '"')
            {
                string value = NextString();
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.String,
                    Value = value
                };
            }
            if (next == ':')
            {
                Advance();
                SkipWhitespace(false); // skip but it's a block so we care about the tabs
                return new Token
                {
                    Type = TokenType.BeginBlock,
                    Value = ":"
                };
            }
            if (GetNext(3) == "str")
            {
                SkipNext(3);
                return new Token
                {
                    Type = TokenType.Str,
                    Value = "str"
                };
            }
            if (GetNext(3) == "int")
            {
                SkipNext(3);
                return new Token
                {
                    Type = TokenType.Int,
                    Value = "int"
                };
            }
            if (GetNext(2) == "b\"" || GetNext(2) == "b'")
            {
                SkipNext(1);
                return new Token
                {
                    Type = TokenType.Bytes,
                    Value = "b"
                };
            }
            if (GetNext(2) == "f\"" || GetNext(2) == "f'")
            {
                SkipNext(1);
                return new Token
                {
                    Type = TokenType.Formatted,
                    Value = "f"
                };
            }
            string numberValue = NextNumber();
            if (numberValue != null)
            {
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.Number,
                    Value = numberValue
                };
            }
            string variableName = NextVariable();
            if (variableName != null)
            {
                SkipWhitespace();
                return new Token
                {
                    Type = TokenType.Variable,
                    Value = variableName
                };
            }
            Console.WriteLine("unknown: " + Source.Substring(Position, 10));
            Console.WriteLine("uh-oh!");
            return null;
        }
        public Keyword NextKeyword()
        {
            string sequence = ReadUntilNext(Keyword.CharacterSet);
            //Console.WriteLine($"Sequence: '{sequence}'");
            if (sequence.Length == 0)
            {
                return null;
            }
            else
            {
                Keyword kw = longestKeywords.FirstOrDefault(k => k.Value == sequence);
                return kw;
            }
        }
        public Operator NextOperator()
        {
            string sequence = ReadUntilNext(Operator.CharacterSet);
            //Console.WriteLine($"Operator: '{sequence}'");
            if (sequence.Length == 0)
            {
                return null;
            }
            else
            {
                Operator op = longestOperators.FirstOrDefault(o => o.Value == sequence);
                if (op == null && sequence[0] == '=')
                {
                    // try the equal sign
                    op = new Operator("=");
                }
                return op;
            }
        }
        // Note: NextString() consumes the characters, don't need to SkipNext()
        public string NextString()
        {
            int start = Position;
            // FIXME this will need to be much smarter about escaped characters
            char opening = GetCurrentCharacter();
            Advance();
            while (GetCurrentCharacter() != opening)
            {
                Advance();
            }
            Advance();
            return Source.Substring(start + 1, (Position - 1) - (start + 1));
        }
        // Note: NextVariable() consumes the characters, don't need to SkipNext()
        public string NextVariable()
        {
            int start = Position, end = start + 1;
            char opening = GetCurrentCharacter();
            bool allowedPrefix = allowedVariablePrefixes.IndexOf(opening) >= 0;
            if (allowedPrefix)
            {
                while (end < Source.Length && allowedVariableCharacters.IndexOf(Source[end]) >= 0)
                {
                    end++;
                }
            }
            else
            {
                return null;
            }
            SkipNext(end - start);
            return Source.Substring(start, end - start);
        }
        public string NextNumber()
        {
            int start = Position, end = start + 1;
            char opening = GetCurrentCharacter();
            bool allowedPrefix = allowedNumberCharacters.IndexOf(opening) >= 0;
            if (allowedPrefix && opening != 'e') // can't start with an e
            {
                while (end < Source.Length && allowedNumberCharacters.IndexOf(Source[end]) >= 0)
                {
                    Console.WriteLine($"CHAR: '{Source[end]}'");
                    end++;
                }
            }
            else
            {
                return null;
            }
            SkipNext(end - start);
            return Source.Substring(start, end - start);
        }
    }
}
