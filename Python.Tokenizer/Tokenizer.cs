using System;
namespace Python.Tokenizer
{
    public class Tokenizer
    {
        public int Position { get; private set; }
        public string Source { get; private set; }
        public int Total => Source.Length;
        public Tokenizer(string source)
        {
            Position = 0;
            Source = source;
        }
        public char GetCurrentCharacter()
        {
            return Source[Position];
        }
        public string ReadUntilWhitespace()
        {
            int start = Position, index = Position;
            while (!IsWhitespace(Source[index]))
            {
                index++;
            }
            return Source.Substring(start, index - start);
        }
        public string ReadUntilNext(char[] valid)
        {
            int start = Position, index = Position;
            while (index < Total)
            {
                char c = Source[index];
                bool isvalid = false;
                for (int idx = 0; idx < valid.Length && !isvalid; idx++)
                {
                    if (valid[idx] == c)
                    {
                        isvalid = true;
                    }
                }
                if (!isvalid)
                {
                    break;
                }
                index++;
            }
            return Source.Substring(start, index - start);
        }
        public bool IsWhitespace(char c, bool tabsAsWhitespace = true)
        {
            return c == ' ' || (tabsAsWhitespace && c == '\t') || c == '\r' || c == '\n';
        }
        public void SkipWhitespace(bool tabsAsWhitespace = true)
        {
            while (IsWhitespace(Source[Position], tabsAsWhitespace))
            {
                Position++;
            }
        }
        public void SkipNext(int n)
        {
            Position += n;
        }
        public void Advance()
        {
            Position++;
        }
    }
}
