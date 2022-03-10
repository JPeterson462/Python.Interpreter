using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class LambdaExpression : Expression
    {
        public List<Expression> Parameters { get; set; }
        public Expression Body { get; set; }
    }
    public class LambdaParameterExpression : Expression
    {
        public Expression Identifier { get; set; }
        public Expression Default { get; set; }
    }
}
