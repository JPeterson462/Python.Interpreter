﻿using System;
namespace Python.Core.Expressions
{
    public class SimpleExpression : Expression
    {
        public string Value { get; set; }
        public bool IsConstant { get; set; }
    }
}