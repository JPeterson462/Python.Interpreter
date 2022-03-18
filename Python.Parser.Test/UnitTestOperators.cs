using Microsoft.VisualStudio.TestTools.UnitTesting;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser.Test
{
    [TestClass]
    public class UnitTestOperators
    {
        [TestMethod]
        public void TestSimpleExpression()
        {
            var expr = (SimpleExpression)TestUtils.ParseExpression("100").AtomSubParser.ParseAtom();
            var expected = TestUtils.SimpleNumber(100);
            Assert.AreEqual((expr as SimpleExpression).IsConstant, expected.IsConstant);
            Assert.AreEqual((expr as SimpleExpression).IsVariable, expected.IsVariable);
            Assert.AreEqual((expr as SimpleExpression).Value, expected.Value);
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestAddition()
        {
            var expr = TestUtils.ParseExpression("3 + 4").OperationSubParser.ParseSum();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = TestUtils.SimpleNumber(3),
                Operator = Operator.Add,
                RightHandValue = TestUtils.SimpleNumber(4)
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestAdditionAndSubtraction()
        {
            var expr = TestUtils.ParseExpression("1 + 2 - 3 + 4").OperationSubParser.ParseSum();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = TestUtils.SimpleNumber(1),
                Operator = Operator.Add,
                RightHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleNumber(2),
                    Operator = Operator.Subtract,
                    RightHandValue = new EvaluatedExpression
                    {
                        LeftHandValue = TestUtils.SimpleNumber(3),
                        Operator = Operator.Add,
                        RightHandValue = TestUtils.SimpleNumber(4)
                    }
                }
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestExponentiation()
        {
            var expr = TestUtils.ParseExpression("2 ** 3").OperationSubParser.ParsePower();
            var expected = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleNumber(2),
                    Operator = Operator.Exponentiation,
                    RightHandValue = TestUtils.SimpleNumber(3)
                };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestAdditionAndExponentiation()
        {
            var expr = TestUtils.ParseExpression("1 + 2 ** 3").OperationSubParser.ParseSum();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = TestUtils.SimpleNumber(1),
                Operator = Operator.Add,
                RightHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleNumber(2),
                    Operator = Operator.Exponentiation,
                    RightHandValue = TestUtils.SimpleNumber(3)
                }
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestManyOperations1()
        {
            //1 - 2 ** -2 + 3 * 4 // 2
            // 1 - (2 ** -2) + (3 * 4 // 2)
            // [1 - (2 ** -2)] + [3 * 4 // 2]
            var expr = TestUtils.ParseExpression("1 - 2 ** -2 + 3 * 4 // 2").OperationSubParser.ParseShiftExpr();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleNumber(1),
                    Operator = Operator.Subtract,
                    RightHandValue = new EvaluatedExpression
                    {
                        LeftHandValue = TestUtils.SimpleNumber(2),
                        Operator = Operator.Exponentiation,
                        RightHandValue = new EvaluatedExpression
                        {
                            Operator = Operator.Subtract,
                            RightHandValue = TestUtils.SimpleNumber(2)
                        }
                    }
                },
                Operator = Operator.Add,
                RightHandValue = new EvaluatedExpression
                {
                    LeftHandValue = new EvaluatedExpression
                    {
                        LeftHandValue = TestUtils.SimpleNumber(3),
                        Operator = Operator.Multiply,
                        RightHandValue = TestUtils.SimpleNumber(4)
                    },
                    Operator = Operator.FloorDivide,
                    RightHandValue = TestUtils.SimpleNumber(2)
                }
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestComparisonAddition()
        {
            var expr = TestUtils.ParseExpression("a + b >= c - d").OperationSubParser.ParseComparison();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleVariable("a"),
                    Operator = Operator.Add,
                    RightHandValue = TestUtils.SimpleVariable("b")
                },
                Operator = Operator.GreaterThanOrEqualTo,
                RightHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleVariable("c"),
                    Operator = Operator.Subtract,
                    RightHandValue = TestUtils.SimpleVariable("d")
                }
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }
        [TestMethod]
        public void TestNotInAnd()
        {
            var expr = TestUtils.ParseExpression("not a in set and b > 0").OperationSubParser.ParseDisjunction();
            var expected = new EvaluatedExpression
            {
                LeftHandValue = new EvaluatedExpression
                {
                    LeftHandValue = null,
                    KeywordOperator = Keyword.Not,
                    RightHandValue = new EvaluatedExpression
                    {
                        LeftHandValue = TestUtils.SimpleVariable("a"),
                        KeywordOperator = Keyword.In,
                        RightHandValue = TestUtils.SimpleVariable("set")
                    }
                },
                KeywordOperator = Keyword.And,
                RightHandValue = new EvaluatedExpression
                {
                    LeftHandValue = TestUtils.SimpleVariable("b"),
                    Operator = Operator.GreaterThan,
                    RightHandValue = TestUtils.SimpleNumber(0)
                }
            };
            if (!expected.Equals(expr))
            {
                Assert.Fail($"{expected} vs {expr}");
            }
        }

        [TestMethod]
        public void TestSimpleArrayAccessor()
        {
            var expr = TestUtils.ParseExpression("arr[3]").AtomSubParser.ParseTPrimary();

            Assert.Fail($"{expr}");
        }
    }
}
