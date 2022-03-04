using System;
namespace Python.Core
{
    public class Operator
    {
        public static readonly Operator Add = new Operator("+");
        public static readonly Operator Subtract = new Operator("-");
        public static readonly Operator Multiply = new Operator("*");
        public static readonly Operator Divide = new Operator("/");
        public static readonly Operator FloorDivide = new Operator("//");
        public static readonly Operator Modulus = new Operator("%");
        public static readonly Operator Exponentiation = new Operator("**");
        public static readonly Operator EqualTo = new Operator("==");
        public static readonly Operator Set = new Operator("=");
        public static readonly Operator NotEqualTo = new Operator("!=");
        public static readonly Operator LessThan = new Operator("<");
        public static readonly Operator GreaterThan = new Operator(">");
        public static readonly Operator LessThanOrEqualTo = new Operator("<=");
        public static readonly Operator GreaterThanOrEqualTo = new Operator(">=");
        public static readonly Operator Is = new Operator("is");
        public static readonly Operator In = new Operator("in");
        public static readonly Operator Not = new Operator("not");

        public static readonly char[] CharacterSet = "+-*/%=!<>isnot".ToCharArray();

        public static readonly Operator[] ALL = new Operator[]
        {
            Add, Subtract, Multiply, Divide, FloorDivide, Modulus, Exponentiation,
            EqualTo, Set, NotEqualTo, LessThan, GreaterThan, LessThanOrEqualTo,
            GreaterThanOrEqualTo, Is, In, Not
        };

        public string Value { get; set; }
        public int Length => Value.Length;
        public Operator(string value)
        {
            Value = value;
        }
    }
}
