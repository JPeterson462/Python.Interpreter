using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core.Expressions;

namespace Python.Core.CodeBlocks
{
    public class FunctionCodeBlock : CodeBlock
    {
        public List<Expression> Decorators = new List<Expression>();
        public string Name { get; set; }
        public bool IsAsynchronous { get; set; }
        public CollectionExpression Parameters { get; set; }

        public override string ToString()
        {
            return $"{(IsAsynchronous ? "async " : "")}{Name ?? "lambda "}({string.Join(", ", Parameters.Elements.Select(e => e.ToString()))})";
        }
    }
}
