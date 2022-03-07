using System;
namespace Python.Core
{
    public enum ConditionalType
    {
        If, Elif, Else, While, For
    }
    public class ConditionalCodeBlock : CodeBlock
    {
        public ConditionalType Type { get; set; }
        public Expression Condition { get; set; }
    }
}
