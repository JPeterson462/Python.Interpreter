using System;

namespace Python.Core
{
    public enum TokenType
    {
        Keyword, BeginBlock, Number, String,
        BeginParameters, EndParameters, BeginList, EndList,
        ObjectReference, Variable, Operator, ElementSeparator,
        Formatted, Bytes, Decorator, Str, Int, DictionaryStart,
        DictionaryEnd, IndentTab, DedentTab, Tab, Comment, ReturnHint,
        EndOfExpression
    }
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int? Count { get; set; }
    }
}
