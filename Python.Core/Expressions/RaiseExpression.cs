using System;
namespace Python.Core.Expressions
{
    public class RaiseExpression : Expression
    {
        public Expression Expression { get; set; }
        public Expression Source { get; set; }
    }
}
