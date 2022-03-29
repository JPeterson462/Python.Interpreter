using System;
using Python.Core;
namespace Python.Parser
{
    public class CompoundSubParser
    {
        public PythonParser Parser { get; set; }
        public CompoundSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //compound_stmt:
        //    | function_def
        //    | if_stmt
        //    | class_def
        //    | with_stmt
        //    | for_stmt
        //    | try_stmt
        //    | while_stmt
        //    | match_stmt
        public Expression ParseCompoundStatement()
        {
            //    | function_def
            if (Parser.Peek().Value == Keyword.If.Value)
            {
                //    | if_stmt
                return ParseIfStmt();
            }
            //    | class_def
            //    | with_stmt
            //    | for_stmt
            //    | try_stmt
            if (Parser.Peek().Value == Keyword.While.Value)
            {
                //    | while_stmt
                return ParseWhileStmt();
            }
            //    | match_stmt
            // FIXME
            return null;
        }

        public ConditionalCodeBlock ParseWhileStmt()
        {
            Parser.Accept(Keyword.While.Value);
            Parser.Advance();
            Expression condition = Parser.ParseNamedExpression();
            Parser.Accept(":");
            Parser.Advance();
            CodeBlock block = Parser.ParseBlock();
            if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Else.Value)
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.While,
                    ChainedBlock = ParseElseStmt(),
                    Statements = block.Statements
                };
            }
            else
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.While,
                    Statements = block.Statements
                };
            }
        }
        public ConditionalCodeBlock ParseIfStmt()
        {
            Parser.Accept(Keyword.If.Value);
            Parser.Advance();
            Expression condition = Parser.ParseNamedExpression();
            Parser.Accept(":");
            Parser.Advance();
            CodeBlock block = Parser.ParseBlock();
            if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Elif.Value)
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.If,
                    ChainedBlock = ParseElifStmt(),
                    Statements = block.Statements
                };
            }
            else if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Else.Value)
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.If,
                    ChainedBlock = ParseElseStmt(),
                    Statements = block.Statements
                };
            }
            else
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.If,
                    Statements = block.Statements
                };
            }
        }
        public ConditionalCodeBlock ParseElifStmt()
        {
            Parser.Accept(Keyword.Elif.Value);
            Parser.Advance();
            Expression condition = Parser.ParseNamedExpression();
            Parser.Accept(":");
            Parser.Advance();
            CodeBlock block = Parser.ParseBlock();
            if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Elif.Value)
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.Elif,
                    ChainedBlock = ParseElifStmt(),
                    Statements = block.Statements
                };
            }
            else if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Else.Value)
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.Elif,
                    ChainedBlock = ParseElseStmt(),
                    Statements = block.Statements
                };
            }
            else
            {
                return new ConditionalCodeBlock
                {
                    Condition = condition,
                    Type = ConditionalType.Elif,
                    Statements = block.Statements
                };
            }
        }
        public ConditionalCodeBlock ParseElseStmt()
        {
            Parser.Accept(Keyword.Else.Value);
            Parser.Advance();
            Parser.Accept(":");
            Parser.Advance();
            CodeBlock block = Parser.ParseBlock();
            return new ConditionalCodeBlock
            {
                Type = ConditionalType.Else,
                Statements = block.Statements
            };
        }
    }
}
