using System;
using System.Collections.Generic;
using System.Linq;

namespace Python.Core.Expressions
{
    public enum CollectionType
    {
        List, Tuple, GeneratedList, GeneratedTuple, Unknown, Slices, UnpackedDictionary,
        Generator
    }
    public class CollectionExpression : Expression
    {
        public List<Expression> Elements { get; set; } = new List<Expression>();
        public CollectionType Type { get; set; }

        public override string ToString()
        {
            var elements = Elements?.Select(elem => elem?.ToString());
            return string.Join(", ", elements);
        }

        public override bool Equals(object other)
        {
            if (other is CollectionExpression collection)
            {
                if (collection.Type != Type)
                {
                    return false;
                }
                if (Elements == null)
                {
                    return Elements == collection.Elements;
                }
                if (Elements.Count != collection.Elements.Count)
                {
                    return false;
                }
                for (int i = 0; i < Elements.Count; i++)
                {
                    if (Elements[i] != collection.Elements[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
