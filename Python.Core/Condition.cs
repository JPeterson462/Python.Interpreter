using System;
namespace Python.Core
{
    public class Condition
    {
        public string LeftHandValue { get; set; }
        public bool IsLeftHandVariable { get; set; }
        public Operator Operator { get; set; }
        public string RightHandValue { get; set; }
        public bool IsRightHandVariable { get; set; }
    }
}
