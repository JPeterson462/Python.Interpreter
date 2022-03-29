using System;
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
            throw new NotImplementedException();
        }
    }
}
