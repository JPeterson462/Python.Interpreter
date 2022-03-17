using System;
namespace Python.Core.Expressions
{
    public class EvaluatedExpression : Expression
    {
        public Expression LeftHandValue { get; set; }
        public Operator Operator { get; set; }
        public Keyword KeywordOperator { get; set; }
        public bool IsObjectReference { get; set; }
        public bool IsArrayAccessor { get; set; }
        public bool IsFunctionCall { get; set; }
        public Expression RightHandValue { get; set; }

        public override bool Equals(object other)
        {
            if (other is EvaluatedExpression expr)
            {
                if (other == null)
                {
                    return false;
                }
                if (LeftHandValue != null)
                {
                    if (!LeftHandValue.Equals(expr.LeftHandValue))
                    {
                        return false;
                    }
                }
                else if (expr.LeftHandValue != null)
                {
                    return false;
                }
                if (RightHandValue != null)
                {
                    if (!RightHandValue.Equals(expr.RightHandValue))
                    {
                        return false;
                    }
                }
                else if (expr.RightHandValue != null)
                {
                    return false;
                }
                if (Operator != expr.Operator)
                {
                    return false;
                }
                if (KeywordOperator != expr.KeywordOperator)
                {
                    return false;
                }
                return IsObjectReference == expr.IsObjectReference && IsArrayAccessor == expr.IsArrayAccessor;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            string rightHandPrefix = Operator?.ToString() ?? (KeywordOperator?.ToString() ?? string.Empty);
            string rightHandSuffix = string.Empty;
            if (IsObjectReference)
            {
                rightHandPrefix += ".";
            }
            if (IsArrayAccessor)
            {
                rightHandPrefix += "[";
                rightHandSuffix += "]";
            }
            return $"({LeftHandValue}){rightHandPrefix}({RightHandValue}){rightHandSuffix}";
        }
    }
}
