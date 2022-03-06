using System;
using System.Collections.Generic;

namespace Python.Core
{
    public class CodeBlock : Expression
    {
        public List<Expression> Statements = new List<Expression>();
    }
}
