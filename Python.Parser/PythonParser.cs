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
            /*while (Position < Tokens.Count)
            {
                
            }*/
            script.Statements.Add(ParseExpression(0, Tokens.Count));
            return script;
        }
        private void AddIndent(int value)
        {
            if (currentIndent >= indents.Count)
            {
                indents.Add(value);
            }
            else
            {
                indents[currentIndent] = value;
            }
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
        public Expression ParseStatement()
        {
            Token token = Tokens[Position];
            if (token.Type == TokenType.Tab)
            {
                // process the indent level
                Advance();
            }
            int endOfExpression = FindNext(TokenType.EndOfExpression);
            return ParseExpression(Position, endOfExpression);
        }
        public Expression ParseExpression(int startPos, int endPos)
        {
            Token token = Tokens[startPos];
            Token nextToken = Tokens[startPos + 1];
            if (token.Type == TokenType.Keyword)
            {
                if (token.Value == Keyword.If.Value)
                {
                    int endOfCondition = FindNext(TokenType.BeginBlock, startPos + 1);
                    Expression condition = ParseExpression(startPos + 1, endOfCondition);
                    ConditionalCodeBlock ifBlock = new ConditionalCodeBlock
                    {
                        Condition = condition,
                        Type = ConditionalType.If
                    };
                    int blockStart = endOfCondition + 1 + 1; // skip the BeginBlock and EndOfExpression
                    bool inBlock = true;
                    while (inBlock)
                    {
                        while (blockStart< Tokens.Count && (Tokens[blockStart].Type == TokenType.EndOfExpression || (Tokens[blockStart].Type == TokenType.Tab && Tokens[blockStart + 1].Type == TokenType.EndOfExpression)))
                        {
                            blockStart += Tokens[blockStart].Type == TokenType.EndOfExpression ? 1 : 2; // empty lines (with or without a tab before)
                        }
                        if (blockStart == Tokens.Count)
                        {
                            // EOF
                            break;
                        }
                        int endOfNextExpression = FindNext(TokenType.EndOfExpression, blockStart);
                        Token tab = Tokens[blockStart];//TODO end of block based on indent
                        ifBlock.Statements.Add(ParseExpression(blockStart + 1, endOfNextExpression));
                        blockStart = endOfNextExpression + 1;
                        if (endOfNextExpression >= Tokens.Count)
                        {
                            inBlock = false;
                        }
                    }
                    return ifBlock;
                }
            }
            if (token.Type == TokenType.Variable && nextToken.Type == TokenType.BeginParameters)
            {
                // function call
                int start = startPos + 2, end = FindEndOfRegion(TokenType.BeginParameters, TokenType.EndParameters, start);
                return new FunctionExpression
                {
                    VariableName = token.Value,
                    Parameters = new List<Expression>(new Expression[] { ParseExpression(start, end) })
                };// FIXME multiple parameters
            }
            if (nextToken.Type == TokenType.EndParameters)
            {
                return new SimpleExpression
                {
                    Value = token.Value,
                    IsConstant = token.Type == TokenType.String || token.Type == TokenType.Number,
                    IsVariable = token.Type == TokenType.Variable
                };
            }
            if ((nextToken.Type == TokenType.BeginBlock || nextToken.Type == TokenType.EndOfExpression) &&
                (token.Type == TokenType.String || token.Type == TokenType.Number || token.Type == TokenType.Variable))
            {
                return new SimpleExpression
                {
                    Value = token.Value,
                    IsConstant = token.Type == TokenType.String || token.Type == TokenType.Number,
                    IsVariable = token.Type == TokenType.Variable
                };
            }
            else
            {
                if (token.Type == TokenType.BeginParameters)
                {
                    int start = startPos, end = FindEndOfRegion(TokenType.BeginParameters, TokenType.EndParameters, start);
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
                        IsConstant = token.Type == TokenType.String || token.Type == TokenType.Number,
                        IsVariable = token.Type == TokenType.Variable
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
