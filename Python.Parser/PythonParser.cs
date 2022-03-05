using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class PythonParser : Parser
    {
        private IEnumerable<Keyword> longestKeywords;
        private IEnumerable<Operator> longestOperators;
        public List<int> indents;
        public int currentIndent;
        public PythonParser(List<Token> tokens) : base(tokens)
        {
            indents = new List<int>();
            indents.Add(0);
            currentIndent = 0;
            longestKeywords = Keyword.ALL.ToList().OrderByDescending(kw => kw.Value.Length);
            longestOperators = Operator.ALL.ToList().OrderByDescending(op => op.Value.Length);
        }
        public Script Parse()
        {
            Script script = new Script();
            while (Position < Tokens.Count)
            {
                Token token = Tokens[Position];
                Console.WriteLine("TOKEN: " + token.Value);
                Advance();
                if (token.Type == TokenType.Keyword && token.Value == Keyword.If.Value)
                {
                    int endOfCondition = FindNext(TokenType.BeginBlock);
                    Expression condition = ParseExpression(Position, endOfCondition);
                    SkipNext(endOfCondition - Position);
                    currentIndent++;
                    if (Tokens[Position].Type == TokenType.BeginBlock)
                    {
                        Advance();
                        script.CodeBlocks.Add(ParseConditionalCodeBlock(condition, ConditionalType.If));
                    }
                }
                break;
            }
            return script;
        }
        public ConditionalCodeBlock ParseConditionalCodeBlock(Expression condition, ConditionalType type)
        {
            ConditionalCodeBlock codeBlock = new ConditionalCodeBlock()
            {
                Condition = condition
            };
            Token possibleTab = Tokens[Position];
            if (currentIndent >= indents.Count)
            {
                indents.Add(possibleTab.Count);
            }
            else
            {
                indents[currentIndent] = possibleTab.Count;
            }
            Advance();

            //Console.WriteLine(Enum.GetName(typeof(TokenType), possibleTab.Type) + " " + possibleTab.Count);
            return codeBlock;
        }
        public Expression ParseExpression(int startPos, int endPos)
        {
            Token token = Tokens[startPos];
            Token nextToken = Tokens[startPos + 1];
            if (nextToken.Type == TokenType.BeginBlock || token.Type == TokenType.String || token.Type == TokenType.Number)
            {
                return new SimpleExpression
                {
                    Value = token.Value,
                    IsConstant = token.Type != TokenType.Keyword
                };
            }
            else
            {
                if (token.Type == TokenType.BeginParameters)
                {
                    int start = Position, end = start + 1, parenthesesCount = 1;
                    while (parenthesesCount > 0)
                    {
                        if (Tokens[end].Type == TokenType.BeginParameters)
                        {
                            parenthesesCount++;
                        }
                        if (Tokens[end].Type == TokenType.EndParameters)
                        {
                            parenthesesCount--;
                        }
                        end++;
                    }
                    Expression leftHandExpression = ParseExpression(start + 1, end - 1);
                    if (end == endPos)
                    {
                        return leftHandExpression;
                    }
                    Operator op = new Operator(Tokens[end].Value);
                    Expression rightHandExpression = ParseExpression(end + 1, endPos);
                    return new EvaluatedExpression
                    {
                        LeftHandValue = leftHandExpression,
                        Operator = op,
                        RightHandValue = rightHandExpression
                    };
                }
                else
                {
                    Expression leftHandExpression = new SimpleExpression
                    {
                        Value = token.Value,
                        IsConstant = token.Type != TokenType.Keyword
                    };
                    Operator op = new Operator(Tokens[startPos + 1].Value);
                    Expression rightHandExpression = ParseExpression(startPos + 2, endPos);
                    return new EvaluatedExpression
                    {
                        LeftHandValue = leftHandExpression,
                        Operator = op,
                        RightHandValue = rightHandExpression
                    };
                }
            }
        }
    }
}
