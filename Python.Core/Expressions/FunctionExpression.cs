using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class FunctionExpression : Expression
    {
        public string VariableName { get; set; }
        public List<Expression> Parameters { get; set; }
    }
}
