using System;
namespace Python.Core.Expressions
{
    public class KeyValueExpression : Expression
    {
        public Expression Key { get; set; }
        public Expression Value { get; set; }
    }
}
