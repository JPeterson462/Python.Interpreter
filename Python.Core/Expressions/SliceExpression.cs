using System;
namespace Python.Core.Expressions
{
    public class SliceExpression : Expression
    {
        /// <summary>
        /// Default value is 0
        /// </summary>
        public Expression Start { get; set; }
        /// <summary>
        /// Default value is len(collection)
        /// </summary>
        public Expression Stop { get; set; }
        /// <summary>
        /// Default value is 1
        /// </summary>
        public Expression Interval { get; set; }
        /// <summary>
        /// If true, Start defines the elements in range. Otherwise, start <= n < stop
        /// </summary>
        public bool IsExpression { get; set; }
    }
}
