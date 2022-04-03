using System;
using System.Collections.Generic;

namespace Python.Core.CodeBlocks
{
    /// <summary>
    /// with 'item' as 'target'
    /// </summary>
    public class WithItem : Expression
    {
        public Expression Item { get; set; }
        public Expression Target { get; set; }
    }
    public class WithCodeBlock : CodeBlock
    {
        public bool IsAsynchronous { get; set; }
        public List<WithItem> WithItems { get; set; }
    }
}
