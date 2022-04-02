using System;
namespace Python.Core
{
    public class Operator
    {
        public static readonly Operator Add = new Operator("+");
        public static readonly Operator Subtract = new Operator("-");
        public static readonly Operator Multiply = new Operator("*");
        public static readonly Operator Divide = new Operator("/");
        public static readonly Operator AddSet = new Operator("+=");
        public static readonly Operator SubtractSet = new Operator("-=");
        public static readonly Operator MultiplySet = new Operator("*=");
        public static readonly Operator DivideSet = new Operator("/=");
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
        public static readonly Operator LeftShift = new Operator("<<");
        public static readonly Operator RightShift = new Operator(">>");
        public static readonly Operator BitwiseAnd = new Operator("&");
        public static readonly Operator BitwiseOr = new Operator("|");
        public static readonly Operator BitwiseNot = new Operator("~");
        public static readonly Operator BitwiseXor = new Operator("^");
        public static readonly Operator LeftShiftSet = new Operator("<<=");
        public static readonly Operator RightShiftSet = new Operator(">>=");
        public static readonly Operator BitwiseAndSet = new Operator("&=");
        public static readonly Operator BitwiseOrSet = new Operator("|=");
        public static readonly Operator BitwiseXorSet = new Operator("^=");
        public static readonly Operator Assignment = new Operator(":=");
        public static readonly Operator ObjectReference = new Operator("."); // this one is parsed as a token and constructed externally
        public static readonly Operator ReturnReference = new Operator("->");

        public static readonly char[] CharacterSet = "+-*/%=!<>&|~^:".ToCharArray();

        public static readonly Operator[] ALL = new Operator[]
        {
            Add, Subtract, Multiply, Divide, FloorDivide, Modulus, Exponentiation,
            EqualTo, Set, NotEqualTo, LessThan, GreaterThan, LessThanOrEqualTo,
            GreaterThanOrEqualTo, AddSet, SubtractSet, MultiplySet, DivideSet,
            LeftShift, RightShift, BitwiseAnd, BitwiseOr, BitwiseNot, BitwiseXor,
            LeftShiftSet, RightShiftSet, BitwiseAndSet, BitwiseOrSet, BitwiseXorSet,
            Assignment, ReturnReference
        };

        public readonly string Value;
        public int Length => Value.Length;
        public Operator(string value)
        {
            Value = value;
        }

        public bool Equals(object other)
        {
            if (other is Operator op)
            {
                if (other == null)
                {
                    return false;
                }
                return op.Value == Value;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
