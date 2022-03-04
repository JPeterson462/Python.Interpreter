using System;

namespace Python.Core
{
    public enum TokenType
    {
        Keyword, BeginBlock, EndBlock, Number, String,
        BeginParameters, EndParameters, BeginList, EndList,
        ObjectReference, Variable, Operator, ElementSeparator,
        Formatted, Bytes, Decorator, Str, Int, DictionaryStart,
        DictionaryEnd
    }
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }
}
