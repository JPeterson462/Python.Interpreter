using System;
namespace Python.Core
{
    public enum ConditionalType
    {
        If
    }
    public class ConditionalCodeBlock : CodeBlock
    {
        public ConditionalType Type { get; set; }
        public Expression Condition { get; set; }
    }
}
