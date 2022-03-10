using System;
using System.Collections.Generic;

namespace Python.Core.CodeBlocks
{
    public class FunctionCodeBlock : CodeBlock
    {
        public List<Expression> Decorators = new List<Expression>();
        public string Name { get; set; }
        public bool IsAsynchronous { get; set; }
    }
}
