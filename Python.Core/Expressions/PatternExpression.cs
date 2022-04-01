using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class Pattern
    {

    }
    public enum SequenceType
    {
        Tuple, List
    }
    public class SequencePattern : Pattern
    {
        public SequenceType Type { get; set; }
        public List<Pattern> Elements { get; set; }
    }
    public class StarPattern : Pattern
    {
        public Pattern Pattern { get; set; }
    }
    public class AttributePattern : Pattern
    {
        public List<string> Parts { get; set; }
    }
    public class OrPattern : Pattern
    {
        public Pattern CaptureTarget { get; set; }
        public List<Pattern> Parts { get; set; }
    }
    public class WildcardPattern : Pattern
    {

    }
    public class VariablePattern : Pattern
    {
        public string Variable { get; set; }
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
