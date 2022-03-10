using System;
using System.Collections.Generic;
using System.IO;
using Python.Core;
using Python.Parser;
using Python.Tokenizer;

namespace Python.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 1 + 2 * 3
            Expression e1 = ParsingUnitTest("1 + 2 * 3\n").OperationSubParser.ParseSum();

            string source = File.ReadAllText("/Users/jpeterson/git/PythonLexer/Python.Interpreter/Python.Interpreter/test8.py");

            PythonTokenizer tokenizer = new PythonTokenizer(source);
            List<Token> tokens = tokenizer.Consume();
            /*DateTime startTime = DateTime.Now;
            Console.WriteLine(startTime + ": start");
            foreach (Token t in tokens)
            {
                //PrintToken(t);
            }
            DateTime endTime = DateTime.Now;
            Console.WriteLine(endTime + ": end");
            TimeSpan difference = endTime.Subtract(startTime);
            Console.WriteLine("Time elapsed: " + difference.Milliseconds + "ms");*/

            PythonParser parser = new PythonParser(tokens);
            Script script = parser.Parse();

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
        public static PythonParser ParsingUnitTest(string source)
        {
            return new PythonParser(new PythonTokenizer(source).Consume());
        }
    }
}
