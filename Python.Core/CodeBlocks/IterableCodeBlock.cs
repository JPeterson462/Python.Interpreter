using System;
using System.Collections.Generic;

namespace Python.Core.CodeBlocks
{
    public class IterableCodeBlock : CodeBlock
    {
        public bool IsAsynchronous { get; set; }
        public List<Expression> Targets { get; set; }
        public List<Expression> Generators { get; set; }
        public ConditionalCodeBlock ChainedCodeBlock { get; set; }
    }
}
