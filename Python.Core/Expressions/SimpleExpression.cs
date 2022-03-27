using System;
namespace Python.Core.Expressions
{
    public class SimpleExpression : Expression
    {
        public string Value { get; set; }
        public Expression Annotation { get; set; }
        public bool IsConstant { get; set; }
        public Type ConstantType { get; set; }
        public bool IsVariable { get; set; }

        public SimpleExpression()
        {

        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }
            if (other is SimpleExpression expr)
            {
                if (!expr.Value.Equals(Value))
                {
                    return false;
                }
                return (expr.IsConstant == IsConstant && expr.IsVariable == IsVariable);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{(IsConstant ? $"{(ConstantType != null ? ConstantType.Name + " " : "")}constant ": "")}{(IsVariable ? "variable " : "")}{(Annotation != null ? Annotation.ToString() + "  " : "")}{Value}";
        }
    }
}
