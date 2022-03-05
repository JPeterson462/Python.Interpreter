using System;
using System.Collections.Generic;
using Python.Core;

namespace Python.Parser
{
    public abstract class Parser
    {
        public List<Token> Tokens { get; set; }
        public int Position { get; private set; }
        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
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
        public int FindNext(TokenType type)
        {
            int pos = Position;
            while (Tokens[pos].Type != type)
            {
                pos++;
            }
            return pos;
        }
    }
}
