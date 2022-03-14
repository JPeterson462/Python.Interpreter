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
    }
}
