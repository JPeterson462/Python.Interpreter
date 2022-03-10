using System;
namespace Python.Core.Expressions
{
    public class TernaryExpression : Expression
    {
        public Expression Condition { get; set; }
        public Expression TrueCase { get; set; }
        public Expression FalseCase { get; set; }
    }
}
