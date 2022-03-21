using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class LambdaParameterExpression : Expression
    {
        public Expression Identifier { get; set; }
        public Expression Default { get; set; }

        public override string ToString()
        {
            return $"{Identifier}{(Default != null ? "="+Default : "")}";
        }
    }
}
