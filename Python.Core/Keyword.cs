using System;
namespace Python.Core
{
    public class Keyword
    {
        // hard keywords
        public static readonly Keyword And = new Keyword("and", false, false);
        public static readonly Keyword As = new Keyword("as", false, false);
        public static readonly Keyword Assert = new Keyword("assert", false, false);
        public static readonly Keyword Async = new Keyword("async", false, false);
        public static readonly Keyword Await = new Keyword("await", false, false);
        public static readonly Keyword Break = new Keyword("break", false, false);
        public static readonly Keyword Class = new Keyword("class", true, false);
        public static readonly Keyword Continue = new Keyword("continue", false, false);
        public static readonly Keyword Def = new Keyword("def", true, false);
        public static readonly Keyword Del = new Keyword("del", false, false);
        public static readonly Keyword Elif = new Keyword("elif", true, true);
        public static readonly Keyword Else = new Keyword("else", true, true);
        public static readonly Keyword Except = new Keyword("except", true, false);
        public static readonly Keyword False = new Keyword("False", false, false);
        public static readonly Keyword Finally = new Keyword("finally", true, false);
        public static readonly Keyword For = new Keyword("for", true, false);
        public static readonly Keyword From = new Keyword("from", false, false);
        public static readonly Keyword Global = new Keyword("global", false, false);
        public static readonly Keyword If = new Keyword("if", true, true);
        public static readonly Keyword Import = new Keyword("import", false, false);
        public static readonly Keyword In = new Keyword("in", false, false);
        public static readonly Keyword Is = new Keyword("is", false, false);
        public static readonly Keyword Lambda = new Keyword("lambda", true, false);
        public static readonly Keyword None = new Keyword("None", false, false);
        public static readonly Keyword Nonlocal = new Keyword("nonlocal", false, false);
        public static readonly Keyword Not = new Keyword("not", false, false);
        public static readonly Keyword Or = new Keyword("or", false, false);
        public static readonly Keyword Pass = new Keyword("pass", false, false);
        public static readonly Keyword Raise = new Keyword("raise", false, false);
        public static readonly Keyword Return = new Keyword("return", false, false);
        public static readonly Keyword True = new Keyword("True", false, false);
        public static readonly Keyword Try = new Keyword("try", true, false);
        public static readonly Keyword While = new Keyword("while", true, true);
        public static readonly Keyword With = new Keyword("with", false, false);
        public static readonly Keyword Yield = new Keyword("yield", false, false);
        // soft keywords
        //public static readonly Keyword UNDERSCORE = new Keyword("_", false, false); // treat this one as a variable
        public static readonly Keyword Case = new Keyword("case", true, true);
        public static readonly Keyword Match = new Keyword("match", true, true);
        //public static readonly Keyword NAME = new Keyword("__name__", false, false); // treat this one as a variable

        public static char[] CharacterSet = "TFNabcdefghilmnoprstuwy_".ToCharArray();

        public static readonly Keyword[] ALL = new Keyword[]
        {
            And, As, Assert, Async, Await, Break, Class, Continue, Def, Del,
            Elif, Else, Except, False, Finally, For, From, Global, If, Import,
            In, Is, Lambda, None, Nonlocal, Not, Or, Pass, Raise, Return, True,
            Try, While, With, Yield, Case, Match //, UNDERSCORE, NAME
        };

        public string Value { get; set; }
        public bool IsBlockDefinition { get; set; }
        public bool IsConditionalBlock { get; set; }
        public int Length => Value.Length;
        public Keyword(string value, bool blockDefinition, bool conditionalBlock)
        {
            Value = value;
            IsBlockDefinition = blockDefinition;
            IsConditionalBlock = conditionalBlock;
        }

        public bool Equals(object other)
        {
            if (other is Keyword kw)
            {
                if (other == null)
                {
                    return false;
                }
                return kw.Value == Value;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{(IsBlockDefinition ? "block " : "")}{(IsConditionalBlock ? "conditional " : "")}{Value}";
        }
    }
}
