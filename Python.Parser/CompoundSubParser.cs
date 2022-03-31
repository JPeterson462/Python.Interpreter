using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.Expressions;

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
            if (Parser.Peek().Value == Keyword.Match.Value)
            {
                //    | match_stmt
                return ParseMatchStmt();
            }
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

        public MatchExpression ParseMatchStmt()
        {
            Parser.Accept(Keyword.Match.Value);
            Parser.Advance();
            Expression subject = ParseSubjectExpr();
            Parser.Accept(":");
            Parser.Advance();
            Parser.Accept("\n");
            Parser.Advance();
            Parser.Accept(TokenType.IndentTab);
            Parser.Advance();
            List<ConditionalCodeBlock> cases = new List<ConditionalCodeBlock>();
            while (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == Keyword.Case.Value)
            {
                cases.Add(ParseCaseStatement());
            }
            if (Parser.Position < Parser.Tokens.Count)
            {
                Parser.Accept(TokenType.DedentTab);
                Parser.Advance();
            }
            return new MatchExpression
            {
                Subject = subject,
                CaseStatements = cases
            };
        }

        //subject_expr:
        //    | ','.star_named_expression+ [','] 
        //    | named_expression
        public Expression ParseSubjectExpr()
        {
            // not sure when a star_named_expression wouldn't match for a named_expression...
            Expression e1 = Parser.ParseStarNamedExpression();
            if (Parser.Peek().Value == ",")
            {
                CollectionExpression collection = new CollectionExpression
                {
                    Type = CollectionType.List
                };
                collection.Elements.Add(e1);
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    collection.Elements.Add(Parser.ParseStarNamedExpression());
                }
                return collection;
            }
            else
            {
                return e1;
            }
        }

        public ConditionalCodeBlock ParseCaseStatement()
        {
            Parser.Accept(Keyword.Case.Value);
            Parser.Advance();
            Expression pattern = ParsePatternsAndGuard();
            Parser.Accept(":");
            Parser.Advance();
            CodeBlock block = Parser.ParseBlock();
            return new ConditionalCodeBlock
            {
                Type = ConditionalType.Case,
                Condition = pattern,
                Statements = block.Statements
            };
        }
        public Expression ParsePatternsAndGuard()
        {
            PatternExpression pattern = Parser.PatternSubParser.ParsePatterns();
            if (Parser.Peek().Value == Keyword.If.Value)
            {
                Parser.Advance();
                pattern.Guard = Parser.ParseNamedExpression();
            }
            return pattern;
        }
    }
}
