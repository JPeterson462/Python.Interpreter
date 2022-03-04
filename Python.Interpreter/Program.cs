using System;
using System.IO;
using Python.Core;
using Python.Tokenizer;

namespace Python.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = File.ReadAllText("/Users/jpeterson/git/PythonLexer/Python.Interpreter/Python.Interpreter/test1.py");
            PythonTokenizer tokenizer = new PythonTokenizer(source);
            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine("NEXT: " + tokenizer.GetCurrentCharacter());
                Token t = tokenizer.NextToken();
                PrintToken(t);
            }
            Console.WriteLine("NEXT: " + tokenizer.GetCurrentCharacter());
            Console.WriteLine("** done **");
            Console.WriteLine($"TEST: {tokenizer.IsWhitespace('\t', false)}");
        }
        private static  void PrintToken(Token t)
        {
            if (t == null)
            {
                Console.WriteLine("type: NULL");
                Console.WriteLine("value: NONE");
            }
            else
            {
                Console.WriteLine("type: " + Enum.GetName(typeof(TokenType), t.Type));
                Console.WriteLine("value: " + t.Value);
            }
        }

    }
}
