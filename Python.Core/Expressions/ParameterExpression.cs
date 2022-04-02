using System;
namespace Python.Core.Expressions
{
    public class ParameterExpression : Expression
    {
        public bool ListGenerator { get; set; }
        public bool DictionaryGenerator { get; set; }
        public string Name { get; set; }
        public Expression Default { get; set; }
        public Expression Annotation { get; set; }
        public bool KeywordOnly { get; set; }
    }
}
