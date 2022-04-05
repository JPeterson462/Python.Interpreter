using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class ForIfGeneratorExpression : Expression
    {
        public bool IsAsynchronous { get; set; }
        public Expression Targets { get; set; }
        public Expression Group { get; set; }
        public List<Expression> Conditions { get; set; }
    }
}
