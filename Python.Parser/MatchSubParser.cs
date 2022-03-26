using System;
namespace Python.Parser
{
    public class MatchSubParser
    {
        public PythonParser Parser { get; set; }
        public MatchSubParser(PythonParser parser)
        {
            Parser = parser;
        }
    }
}
