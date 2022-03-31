using System;
namespace Python.Core.Expressions
{
    public class Pattern
    {

    }
    public class NonePattern : Pattern
    {

    }
    public class BooleanPattern : Pattern
    {
        public bool Value { get; set; }
    }
    public class StringPattern : Pattern
    {
        public string Value { get; set; }
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
