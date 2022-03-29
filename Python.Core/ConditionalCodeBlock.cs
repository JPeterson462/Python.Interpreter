using System;
namespace Python.Core
{
    public enum ConditionalType
    {
        If, Elif, Else, While, For, Case
    }
    public class ConditionalCodeBlock : CodeBlock
    {
        public ConditionalType Type { get; set; }
        public Expression Condition { get; set; }
        public ConditionalCodeBlock ChainedBlock { get; set; }

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(ConditionalType), Type)} {Condition}";
        }
    }
}
