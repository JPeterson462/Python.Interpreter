using System;
namespace Python.Core.Expressions
{
    public class Pattern
    {

    }
    public class NumberPattern : Pattern
    {
        public double RealPart { get; set; }
        public double ImaginaryPart { get; set; }
    }
    public class PatternExpression : Expression
    {
        public Pattern Pattern { get; set; }
        public Expression Guard { get; set; }
    }
}
