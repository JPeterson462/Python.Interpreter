using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class YieldExpression : Expression
    {
        public Expression CollectionExpression { get; set; }
        public List<Expression> Expressions { get; set; }
    }
}
