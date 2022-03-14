using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;
using Python.Core.Expressions;
using Python.Tokenizer;

namespace Python.Parser.Test
{
    public class TestUtils
    {
        public static PythonParser ParseExpression(string val)
        {
            List<Token> tokens = new PythonTokenizer(val).Consume();
            return new PythonParser(tokens);
        }

        public static SimpleExpression SimpleNumber(int val)
        {
            return new SimpleExpression
            {
                Value = val.ToString(),
                IsConstant = true,
                IsVariable = false
            };
        }

        public static SimpleExpression SimpleVariable(string val)
        {
            return new SimpleExpression
            {
                Value = val,
                IsConstant = false,
                IsVariable = true
            };
        }

        public static EvaluatedExpression Calculation(Expression lhs, Operator op, Expression rhs)
        {
            return new EvaluatedExpression
            {
                LeftHandValue = lhs,
                RightHandValue = rhs,
                Operator = op
            };
        }
    }
}
