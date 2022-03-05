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
    }
}
