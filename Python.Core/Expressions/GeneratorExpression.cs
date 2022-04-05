using System;
namespace Python.Core.Expressions
{
    public class GeneratorExpression : Expression
    {
        public Expression Target { get; set; }
        public Expression Generator { get; set; }
    }
}
