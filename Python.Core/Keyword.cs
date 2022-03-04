using System;
namespace Python.Core
{
    public class Keyword
    {
        // hard keywords
        public static readonly Keyword And = new Keyword("and");
        public static readonly Keyword As = new Keyword("as");
        public static readonly Keyword Assert = new Keyword("assert");
        public static readonly Keyword Async = new Keyword("async");
        public static readonly Keyword Await = new Keyword("await");
        public static readonly Keyword Break = new Keyword("break");
        public static readonly Keyword Class = new Keyword("class");
        public static readonly Keyword Continue = new Keyword("continue");
        public static readonly Keyword Def = new Keyword("def");
        public static readonly Keyword Del = new Keyword("del");
        public static readonly Keyword Elif = new Keyword("elif");
        public static readonly Keyword Else = new Keyword("else");
        public static readonly Keyword Except = new Keyword("except");
        public static readonly Keyword False = new Keyword("False");
        public static readonly Keyword Finally = new Keyword("finally");
        public static readonly Keyword For = new Keyword("for");
        public static readonly Keyword From = new Keyword("from");
        public static readonly Keyword Global = new Keyword("global");
        public static readonly Keyword If = new Keyword("if");
        public static readonly Keyword Import = new Keyword("import");
        public static readonly Keyword In = new Keyword("in");
        public static readonly Keyword Is = new Keyword("is");
        public static readonly Keyword Lambda = new Keyword("lambda");
        public static readonly Keyword None = new Keyword("None");
        public static readonly Keyword Nonlocal = new Keyword("nonlocal");
        public static readonly Keyword Not = new Keyword("not");
        public static readonly Keyword Or = new Keyword("or");
        public static readonly Keyword Pass = new Keyword("pass");
        public static readonly Keyword Raise = new Keyword("raise");
        public static readonly Keyword Return = new Keyword("return");
        public static readonly Keyword True = new Keyword("True");
        public static readonly Keyword Try = new Keyword("try");
        public static readonly Keyword While = new Keyword("while");
        public static readonly Keyword With = new Keyword("with");
        public static readonly Keyword Yield = new Keyword("yield");
        // soft keywords
        public static readonly Keyword UNDERSCORE = new Keyword("_");
        public static readonly Keyword Case = new Keyword("case");
        public static readonly Keyword Match = new Keyword("match");
        public static readonly Keyword NAME = new Keyword("__name__");

        public static char[] CharacterSet = "TFNabcdefghilmnoprstuwy_".ToCharArray();

        public static readonly Keyword[] ALL = new Keyword[]
        {
            And, As, Assert, Async, Await, Break, Class, Continue, Def, Del,
            Elif, Else, Except, False, Finally, For, From, Global, If, Import,
            In, Is, Lambda, None, Nonlocal, Not, Or, Pass, Raise, Return, True,
            Try, While, With, Yield, UNDERSCORE, Case, Match, NAME
        };

        public string Value { get; set; }
        public int Length => Value.Length;
        public Keyword(string value)
        {
            Value = value;
        }
    }
}
