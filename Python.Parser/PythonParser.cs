using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;

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
                    Condition condition = ParseSimpleCondition();
                    SkipNext(3);
                    currentIndent++;
                    if (Tokens[Position].Type == TokenType.BeginBlock)
                    {
                        Advance();
                        script.CodeBlocks.Add(ParseConditionalCodeBlock(condition));
                    }
                }
                break;
            }
            return script;
        }
        public ConditionalCodeBlock ParseConditionalCodeBlock(Condition condition)
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
        public Statement ParseStatement()
        {
            return null;
        }
        public Condition ParseSimpleCondition()
        {
            Token leftHand = Tokens[Position + 0];
            Token op = Tokens[Position + 1];
            Token rightHand = Tokens[Position + 2];
            if (op.Type == TokenType.Operator)
            {
                return new Condition
                {
                    LeftHandValue = leftHand.Value,
                    RightHandValue = rightHand.Value,
                    Operator = longestOperators.FirstOrDefault(o => o.Value == op.Value),
                    IsLeftHandVariable = leftHand.Type == TokenType.Keyword,
                    IsRightHandVariable = rightHand.Type == TokenType.Keyword
                };
            }
            return null;
        }
    }
}
