using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public enum CollectionType
    {
        List, Tuple, GeneratedList, Unknown
    }
    public class CollectionExpression : Expression
    {
        public List<Expression> Elements { get; set; } = new List<Expression>();
        public CollectionType Type { get; set; }
    }
}
