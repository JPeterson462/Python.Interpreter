using System;
using System.Collections.Generic;

namespace Python.Core.CodeBlocks
{
    public class TryCatchCodeBlock : CodeBlock
    {
        public List<CatchCodeBlock> CatchBlocks { get; set; }
        public CodeBlock FinallyBlock { get; set; }
        public CodeBlock ElseBlock { get; set; }
    }
    public class CatchCodeBlock : CodeBlock
    {
        public Expression Capture { get; set; }
        public string Alias { get; set; }
    }
}
