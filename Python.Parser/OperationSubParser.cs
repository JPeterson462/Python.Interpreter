using System;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class OperationSubParser
    {
        public PythonParser Parser { get; set; }
        public OperationSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        public Expression ParseDisjunction()
        {
            Expression conjunction = ParseConjunction();
            if (Parser.Peek().Value == Keyword.Or.Value)
            {
                Parser.Advance();
                Expression other = ParseDisjunction(); // recurse to iterate through N elements
                return new EvaluatedExpression
                {
                    LeftHandValue = conjunction,
                    KeywordOperator = Keyword.Or,
                    RightHandValue = other
                };
            }
            else
            {
                return conjunction;
            }
        }
        public Expression ParseConjunction()
        {
            Expression inversion = ParseInversion();
            if (Parser.Peek().Value == Keyword.And.Value)
            {
                Parser.Advance();
                Expression other = ParseConjunction();
                return new EvaluatedExpression
                {
                    LeftHandValue = inversion,
                    KeywordOperator = Keyword.And,
                    RightHandValue = other
                };
            }
            else
            {
                return inversion;
            }
        }
        public Expression ParseInversion()
        {
            if (Parser.Peek().Value == Keyword.Not.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    KeywordOperator = Keyword.Not,
                    RightHandValue = ParseInversion()
                };
            }
            else
            {
                return ParseComparison();
            }
        }
        public Expression ParseComparison()
        {
            Expression expression = ParseBitwiseOr();
            string next = Parser.Peek().Value;
            if (next == Operator.EqualTo.Value || next == Operator.NotEqualTo.Value ||
                next == Operator.LessThanOrEqualTo.Value || next == Operator.GreaterThanOrEqualTo.Value ||
                next == Operator.LessThan.Value || next == Operator.GreaterThan.Value ||
                next == Keyword.Not.Value || next == Keyword.In.Value || next == Keyword.Is.Value)
            {
                throw new NotImplementedException();
            }
            else
            {
                return expression;
            }
        }
        // FIXME probably going to have to reverse any recursion for operator precedence L-to-R
        public Expression ParseBitwiseOr()
        {
            Expression expression = ParseBitwiseXor();
            if (Parser.Peek().Value == Operator.BitwiseOr.Value)
            {
                Parser.Advance();
                expression = new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.BitwiseOr,
                    RightHandValue = ParseBitwiseOr()
                };
            }
            return expression;
        }
        public Expression ParseBitwiseXor()
        {
            Expression expression = ParseBitwiseAnd();
            if (Parser.Peek().Value == Operator.BitwiseXor.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.BitwiseXor,
                    RightHandValue = ParseBitwiseXor()
                };
            }
            return expression;
        }
        public Expression ParseBitwiseAnd()
        {
            Expression expression = ParseShiftExpr();
            if (Parser.Peek().Value == Operator.BitwiseAnd.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.BitwiseAnd,
                    RightHandValue = ParseBitwiseAnd()
                };
            }
            return expression;
        }
        public Expression ParseShiftExpr()
        {
            Expression expression = ParseSum();
            if (Parser.Peek().Value == Operator.LeftShift.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.LeftShift,
                    RightHandValue = ParseShiftExpr()
                };
            }
            if (Parser.Peek().Value == Operator.RightShift.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.RightShift,
                    RightHandValue = ParseShiftExpr()
                };
            }
            return expression;
        }
        public Expression ParseSum()
        {
            Expression expression = ParseTerm();
            if (Parser.Peek().Value == Operator.Add.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.Add,
                    RightHandValue = ParseSum()
                };
            }
            if (Parser.Peek().Value == Operator.Subtract.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.Subtract,
                    RightHandValue = ParseSum()
                };
            }
            return expression;

        }
        public Expression ParseTerm()
        {
            Expression expression = ParseFactor();
            if (Parser.Peek().Value == Operator.Multiply.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.Multiply,
                    RightHandValue = ParseFactor()
                };
            }
            if (Parser.Peek().Value == Operator.Divide.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.Divide,
                    RightHandValue = ParseFactor()
                };
            }
            if (Parser.Peek().Value == Operator.FloorDivide.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.FloorDivide,
                    RightHandValue = ParseFactor()
                };
            }
            if (Parser.Peek().Value == Operator.Modulus.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = expression,
                    Operator = Operator.Modulus,
                    RightHandValue = ParseFactor()
                };
            }
            // TODO what is the @ operator?
            /*if (Parser.Peek().Value == Operato.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.Multiply,
                    RightHandValue = ParseFactor()
                };
            }*/
            return expression;
        }
        public Expression ParseFactor()
        {
            if (Parser.Peek().Value == Operator.Add.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.Add,
                    RightHandValue = ParseFactor()
                };
            }
            else if (Parser.Peek().Value == Operator.Subtract.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.Subtract,
                    RightHandValue = ParseFactor()
                };
            }
            else if (Parser.Peek().Value == Operator.BitwiseNot.Value)
            {
                Parser.Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.BitwiseNot,
                    RightHandValue = ParseFactor()
                };
            }
            else
            {
                return ParsePower();
            }
        }
        public Expression ParsePower()
        {
            Expression primary = ParseAwaitPrimary();
            if (Parser.Peek().Value == Operator.Exponentiation.Value)
            {
                return new EvaluatedExpression
                {
                    LeftHandValue = primary,
                    Operator = Operator.Exponentiation,
                    RightHandValue = ParseFactor()
                };
            }
            else
            {
                return primary;
            }
        }
        public Expression ParseAwaitPrimary()
        {
            if (Parser.Peek().Value == Keyword.Await.Value)
            {
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    KeywordOperator = Keyword.Await,
                    RightHandValue = ParsePrimary()
                };
            }
            else
            {
                return ParsePrimary();
            }
        }
        public Expression ParsePrimary()
        {
            Expression atom = Parser.AtomSubParser.ParseAtom();

            //FIXME
            return atom;
        }
    }
}
