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
        public Expression RightHandValue { get; set; }
    }
}
