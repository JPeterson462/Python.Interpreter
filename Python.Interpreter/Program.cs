using System;
using System.Collections.Generic;
using System.IO;
using Python.Core;
using Python.Core.CodeBlocks;
using Python.Core.Expressions;
using Python.Parser;
using Python.Tokenizer;

namespace Python.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime st = DateTime.UtcNow;
            PythonParser p = ParsingUnitTest("def runme():\n\ta = P @ (V @ M)");

            //PrintTokens(p.Tokens);

            DateTime parstst = DateTime.UtcNow;
            var e38 = p.CompoundSubParser.ParseFunctionDef();

            DateTime en = DateTime.UtcNow;
            TimeSpan parseoffset = en.Subtract(parstst);
            Console.WriteLine("parser time: " + parseoffset);

            Console.WriteLine("** done **");
        }
        private static void PrintTokens(List<Token> tokens)
        {
            foreach (Token t in tokens)
            {
                PrintToken(t);
            }
        }
        private static void PrintToken(Token t)
        {
            Console.WriteLine("[" + Enum.GetName(typeof(TokenType), t.Type) + "'" + t.Value + "']");
        }
        public static PythonParser ParsingUnitTest(string source)
        {
            DateTime st = DateTime.UtcNow;

            PythonParser p = new PythonParser(new PythonTokenizer(source).Consume());

            DateTime en = DateTime.UtcNow;
            TimeSpan offset = en.Subtract(st);
            Console.WriteLine("tokenizer time: " + offset);

            return p;
        }
    }
}
