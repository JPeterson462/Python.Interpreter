using System;
using System.Collections.Generic;
using System.IO;
using Python.Core;
using Python.Tokenizer;

namespace Python.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = File.ReadAllText("/Users/jpeterson/git/PythonLexer/Python.Interpreter/Python.Interpreter/test6.py");
            PythonTokenizer tokenizer = new PythonTokenizer(source);
            List<Token> tokens = tokenizer.Consume();
            DateTime startTime = DateTime.Now;
            Console.WriteLine(startTime + ": start");
            foreach (Token t in tokens)
            {
                //PrintToken(t);
            }
            DateTime endTime = DateTime.Now;
            Console.WriteLine(endTime + ": end");
            TimeSpan difference = endTime.Subtract(startTime);
            Console.WriteLine("Time elapsed: " + difference.Milliseconds + "ms");

            Console.WriteLine("** done **");
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
