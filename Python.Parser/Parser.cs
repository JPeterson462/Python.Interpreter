using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;

namespace Python.Parser
{
    public abstract class Parser
    {
        public List<Token> Tokens { get; set; }
        public int Position { get; private set; }
        public Parser(List<Token> tokens)
        {
            Tokens = tokens.Where(t => t.Type != TokenType.Comment).ToList(); // strip comments before parsing
            Position = 0;
        }
        public void Advance()
        {
            Position++;
        }
        public void SkipNext(int n)
        {
            Position += n;
        }
        /*public List<Token> ReadUntil(TokenType type, bool consume = true)
        {
            List<Token> tokens = new List<Token>();
            int pos = Position;
            while (Tokens[pos].Type != type) {
                tokens.Add(Tokens[pos]);
                pos++;
                if (consume)
                {
                    Advance();
                }
            }
            return tokens;
        }*/
        public int FindNext(TokenType type, int position = -1)
        {
            int pos = position >= 0 ? position : Position;
            while (pos < Tokens.Count && Tokens[pos].Type != type) // stop when we get to the token or the EOF
            {
                pos++;
            }
            return pos;
        }
    }
}
