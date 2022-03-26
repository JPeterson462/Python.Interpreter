using System;
namespace Python.Core.Expressions
{
    public class OperatorExpression : Expression
    {
        public Keyword KeywordOperator { get; set; }
        public Operator Operator { get; set; }
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return $"{(KeywordOperator != null ? KeywordOperator.ToString() : Operator.ToString())}{Expression}";
        }
    }
}
