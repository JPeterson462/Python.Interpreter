using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    /// <summary>
    /// NOTE: this is a function reference, NOT a function definition
    /// </summary>
    public class FunctionExpression : Expression
    {
        public string VariableName { get; set; }
        public List<Expression> Parameters { get; set; }
    }
}
