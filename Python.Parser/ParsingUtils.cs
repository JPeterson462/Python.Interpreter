using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class ParsingUtils
    {
        /// <summary>
        /// flip the tree for left-to-right evaluation
        /// </summary>
        public static Expression FlipExpressionTree(Expression expression, Func<string, bool> acceptOperator)
        {
            //
            //      /\                /\
            //       /\              /\
            //        /\     ->     /\
            //         /\          /\
            //
            List<Expression> tree = new List<Expression>();
            List<Keyword> keywords = new List<Keyword>();
            List<Operator> operators = new List<Operator>();
            Expression ex = expression;
            while (ex is EvaluatedExpression eval && acceptOperator(eval.Operator?.Value ?? eval.KeywordOperator?.Value))
            {
                tree.Add(eval.LeftHandValue);
                keywords.Add(eval.KeywordOperator);
                operators.Add(eval.Operator);
                ex = eval.RightHandValue;
            }
            if (ex is EvaluatedExpression rest)
            {
                tree.Add(rest.RightHandValue);
            }
            else
            {
                tree.Add(ex);
            }
            if (tree.Count == 1)
            {
                return tree[0];
            }
            else
            {
                Expression flipped = new EvaluatedExpression
                {
                    LeftHandValue = tree[0],
                    Operator = operators[0],
                    KeywordOperator = keywords[0],
                    RightHandValue = tree[1]
                };
                for (int i = 1; i < tree.Count - 1; i++)
                {
                    flipped = new EvaluatedExpression
                    {
                        LeftHandValue = flipped,
                        Operator = operators[i],
                        KeywordOperator = keywords[i],
                        RightHandValue = tree[i + 1]
                    };
                }
                return flipped;
            }
        }
    }
}
