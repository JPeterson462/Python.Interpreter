﻿using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    // TODO:
    // lists, dictionaries, keyword handling, object references, formatted, bytes, int, str
    /*
        And, As, Assert, Async, Await, Break, Class, Continue, Def, Del,
        Elif, Else, Except, False, Finally, For, From, Global, If (done), Import,
        In, Is, Lambda, None, Nonlocal, Not, Or, Pass, Raise, Return, True,
        Try, While, With, Yield, Case, Match
     */
    // in-progress: code block parsing recursively
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
            int start = 0;
            while (start < Tokens.Count)
            {
                if (Tokens[start].Type == TokenType.EndOfExpression)
                {
                    // ignore blank lines
                    start++;
                    continue;
                }
                int startOfBlock = FindNext(TokenType.BeginBlock, start);
                int endOfLine = FindNext(TokenType.EndOfExpression, start);
                if (endOfLine < startOfBlock)
                {
                    script.Statements.Add(ParseExpression(start, endOfLine));
                }
                start = endOfLine + 1;
            }
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
                ConditionalType? type = null;
                if (token.Value == Keyword.If.Value)
                {
                    type = ConditionalType.If;
                }
                if (token.Value == Keyword.Elif.Value)
                {
                    type = ConditionalType.Elif;
                }
                if (token.Value == Keyword.Else.Value)
                {
                    type = ConditionalType.Else;
                }
                if (type.HasValue)
                {
                    int endOfCondition = FindNext(TokenType.BeginBlock, startPos + 1);
                    Expression condition = ParseExpression(startPos + 1, endOfCondition);
                    ConditionalCodeBlock ifBlock = new ConditionalCodeBlock
                    {
                        Condition = condition,
                        Type = type.Value
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
                List<Expression> parameters = new List<Expression>();
                int parameterStart = start;
                while (parameterStart < end)
                {
                    int parameterEnd = parameterStart + 1, paramCount = 0, bracketCount = 0, braceCount = 0;
                    while (parameterEnd < end && ((paramCount > 0 || bracketCount > 0 || braceCount > 0) ||
                        (Tokens[parameterEnd].Type != TokenType.ElementSeparator && Tokens[parameterEnd].Type != TokenType.EndParameters)))
                    {
                        // read until the end of the parameter, making sure any opened parentheses/braces/brackets are closed
                        if (Tokens[parameterEnd].Type == TokenType.BeginParameters)
                        {
                            paramCount++;
                        }
                        if (Tokens[parameterEnd].Type == TokenType.EndParameters)
                        {
                            paramCount--;
                        }
                        if (Tokens[parameterEnd].Type == TokenType.DictionaryStart)
                        {
                            braceCount++;
                        }
                        if (Tokens[parameterEnd].Type == TokenType.DictionaryEnd)
                        {
                            braceCount++;
                        }
                        if (Tokens[parameterEnd].Type == TokenType.BeginList)
                        {
                            bracketCount++;
                        }
                        if (Tokens[parameterEnd].Type == TokenType.EndList)
                        {
                            bracketCount++;
                        }
                        parameterEnd++;
                    }
                    parameters.Add(ParseExpression(parameterStart, parameterEnd));
                    parameterStart = parameterEnd + 1;
                }
                return new FunctionExpression
                {
                    VariableName = token.Value,
                    Parameters = parameters
                };
            }
            if (startPos + 1 == endPos)
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
