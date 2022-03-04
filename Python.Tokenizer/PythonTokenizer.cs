using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;

namespace Python.Tokenizer
{
    public class PythonTokenizer : Tokenizer
    {
        private IEnumerable<Keyword> longestKeywords;
        private IEnumerable<Operator> longestOperators;
        public PythonTokenizer(string source) : base(source)
        {
            longestKeywords = Keyword.ALL.ToList().OrderByDescending(kw => kw.Value.Length);
            longestOperators = Operator.ALL.ToList().OrderByDescending(op => op.Value.Length);
        }
        public Token NextToken()
        {
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
                Console.WriteLine($"Next: '{GetCurrentCharacter()}'");
                SkipWhitespace(false); // skip but it's a block so we care about the tabs
                return new Token
                {
                    Type = TokenType.BeginBlock,
                    Value = null
                };
            }

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
            //Console.WriteLine($"Sequence: '{sequence}'");
            if (sequence.Length == 0)
            {
                return null;
            }
            else
            {
                Operator op = longestOperators.FirstOrDefault(o => o.Value == sequence);
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

    }
}
