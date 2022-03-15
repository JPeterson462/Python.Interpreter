using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class ArgumentsExpression : Expression
    {
        public List<Expression> Values { get; set; } = new List<Expression>();
    }

    public class ArgumentExpression : Expression
    {
        public string Name { get; set; }
        public bool UnpackDictionary { get; set; }
        public bool UnpackIterable { get; set; }
        public Expression Expression { get; set; }
    }
}
