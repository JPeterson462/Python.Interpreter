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
        public void RewindTo(int position)
        {
            Position = position;
        }
        public void Advance(int n = 1)
        {
            Position += n;
        }
        public void Accept(TokenType type)
        {
            if (Peek().Type != type)
            {
                ThrowSyntaxError(Position);
            }
        }
        public void Accept(string value)
        {
            if (Peek().Value != value)
            {
                ThrowSyntaxError(Position);
            }
        }
        /*
        public int FindNext(TokenType type, int position = -1)
        {
            int pos = position >= 0 ? position : Position;
            while (pos < Tokens.Count && Tokens[pos].Type != type) // stop when we get to the token or the EOF
            {
                pos++;
            }
            return pos;
        }
        public bool Contains(string value, int start, int end)
        {
            int pos = start;
            while (pos < end)
            {
                if (Tokens[pos].Value == value)
                {
                    return true;
                }
                pos++;
            }
            return false;
        }
        public int FindEndOfRegion(TokenType nestedType, TokenType endOfNestedType, int start)
        {
            int end = start + 1, count = 1;
            while (count > 0)
            {
                if (Tokens[end].Type == nestedType)
                {
                    count++;
                }
                if (Tokens[end].Type == endOfNestedType)
                {
                    count--;
                }
                end++;
            }
            return end;
        }
        */
        public void ThrowSyntaxError(int position)
        {
            throw new Exception("Syntax error!");
        }
        public Token Peek(int n = 0)
        {
            return Tokens[Position + n];
        }        
    }
}
