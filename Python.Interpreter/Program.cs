﻿using System;
using System.Collections.Generic;
using System.IO;
using Python.Core;
using Python.Core.CodeBlocks;
using Python.Parser;
using Python.Tokenizer;

namespace Python.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime st = DateTime.UtcNow;
            /*   // 1. 1 + 2 * 3
               Expression e1 = ParsingUnitTest("1 + 2 * 3\n").OperationSubParser.ParseSum();
               // 2. (3 + 4) * 2
               Expression e2 = ParsingUnitTest("(3 + 4) * 2\n").OperationSubParser.ParseSum();
               // 3. [1, 2, 3]
               Expression e3 = ParsingUnitTest("[1, 2, 3]\n").AtomSubParser.ParseAtom();
               // 4. 1 if True else 0
               Expression e4 = ParsingUnitTest("1 if True else 0\n").ParseExpression();
               // 5. a > b
               Expression e5 = ParsingUnitTest("a > b\n").ParseExpression();
               // 6. 1 + 3 + 5
               Expression e6 = ParsingUnitTest("1 + 3 + 5\n").ParseExpression();
               // 7. 1 - 3 + 5
               Expression e7 = ParsingUnitTest("1 - 3 + 5\n").ParseExpression();
               // 8. yield abc, otherval
               Expression e8 = ParsingUnitTest("yield abc, otherval\n").OperatorSubParser.ParseSimpleStmt();// not working
               // 9. pass
               Expression e9 = ParsingUnitTest("pass\n").OperatorSubParser.ParseSimpleStmt();
               // 10. raise Exception("msg")
               Expression e10 = ParsingUnitTest("raise Exception(\"msg\")\n").OperatorSubParser.ParseSimpleStmt(); // not working
               // 11. raise ex
               Expression e11 = ParsingUnitTest("raise ex\n").OperatorSubParser.ParseSimpleStmt();
               // 12. import utils, sys.io as io
               Expression e12 = ParsingUnitTest("import utils, sys.io as io\n").OperatorSubParser.ParseSimpleStmt();
               // 13. from .bar import Bar
               Expression e13 = ParsingUnitTest("from .bar import Bar\n").OperatorSubParser.ParseSimpleStmt();
               // 14. return a, b
               Expression e14 = ParsingUnitTest("return a, b\n").OperatorSubParser.ParseSimpleStmt();
               // 15. 1:3
               // 16. 1::-1
               // 17. :2
               Expression e15 = ParsingUnitTest("1:3\n").AtomSubParser.ParseSlices();
               Expression e16 = ParsingUnitTest("1::-1\n").AtomSubParser.ParseSlices(); // not working
               Expression e17 = ParsingUnitTest(":2\n").AtomSubParser.ParseSlices();*/

            //Expression e18 = ParsingUnitTest("1 - 2 ** -2 + 3 * 4 // 2\n").OperationSubParser.ParseShiftExpr();

            //Expression e12 = ParsingUnitTest("import utils, sys.io as io\n").OperatorSubParser.ParseSimpleStmt(); // 45ms

            //Expression e19 = ParsingUnitTest("arr[3][1]").AtomSubParser.ParseTPrimary(); // 51ms
            /*Expression e20 = ParsingUnitTest("elem.val.subval").AtomSubParser.ParseTPrimary();
            Expression e21 = ParsingUnitTest("func(abc)(eyy)").AtomSubParser.ParseTPrimary();
            Expression e22 = ParsingUnitTest("func(abc)[0:3]").AtomSubParser.ParseTPrimary();*/

            //Expression e23 = ParsingUnitTest("(arr[3] + arr[1])[0]").ParseExpression(); // 49ms

            //PythonParser p = ParsingUnitTest("lambda x, y = 100, **rest: x * y");

            //PythonParser p = ParsingUnitTest("\ta = 1\n\tb = 1.5\nc = 0\n");

            //PythonParser p = ParsingUnitTest("del a, b.x, c[0]");
            //PythonParser p = ParsingUnitTest("a = 100 * c[0]");

            //PythonParser p = ParsingUnitTest("if x > 0:\n\ta = x\nelif x < 0:\n\ta = -1\nelse:\n\ta = 0");
            PythonParser p = ParsingUnitTest("if x > 0:\n\ty = x\n\tx += 1");

            //PrintTokens(p.Tokens);

            DateTime parstst = DateTime.UtcNow;

            //Expression e24 = p.LambdaSubParser.ParseLambdef(); // 80ms
            //Expression e25 = p.ParseBlock();
            //Expression e26 = p.OperatorSubParser.ParseSimpleStmt();
            //Expression e27 = p.OperatorSubParser.ParseSimpleStmt();
            Expression e28 = p.CompoundSubParser.ParseCompoundStatement();

            DateTime en = DateTime.UtcNow;
            TimeSpan parseoffset = en.Subtract(parstst);
            Console.WriteLine("parser time: " + parseoffset);

            TimeSpan offset = en.Subtract(st);
            //string source = File.ReadAllText("/Users/jpeterson/git/PythonLexer/Python.Interpreter/Python.Interpreter/test8.py");
            Console.WriteLine("total time: " + offset);

            /*PythonTokenizer tokenizer = new PythonTokenizer(source);
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

            //PythonParser parser = new PythonParser(tokens);
            //Script script = parser.Parse();

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
            DateTime st = DateTime.UtcNow;

            PythonParser p = new PythonParser(new PythonTokenizer(source).Consume());

            DateTime en = DateTime.UtcNow;
            TimeSpan offset = en.Subtract(st);
            Console.WriteLine("tokenizer time: " + offset);

            return p;
        }
    }
}
