using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.CodeBlocks;
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
            if (Parser.Peek().Value == "@" || (Parser.Peek().Value == Keyword.Async.Value && Parser.Peek(1).Value == Keyword.Def.Value)
                || Parser.Peek().Value == Keyword.Def.Value)
            {
                //    | function_def
                return ParseFunctionDef();
            }
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
        //function_def:
        //   | decorators function_def_raw 
        //   | function_def_raw
        public FunctionCodeBlock ParseFunctionDef()
        {
            List<Expression> decorators = ParseDecorators();
            FunctionCodeBlock block = ParseFunctionDefRaw();
            block.Decorators = decorators;
            return block;
        }
        //function_def_raw:
        //    | 'def' NAME '(' [params] ')' ['->' expression] ':' [func_type_comment] block 
        //    | ASYNC 'def' NAME '(' [params] ')' ['->' expression] ':' [func_type_comment] block
        public FunctionCodeBlock ParseFunctionDefRaw()
        {
            bool isAsync = false;
            if (Parser.Peek().Value == Keyword.Async.Value)
            {
                Parser.Advance();
                isAsync = true;
            }
            Parser.Accept(Keyword.Def.Value);
            Parser.Advance();
            string name = Parser.Peek().Value;
            Parser.Advance();
            Parser.Accept("(");
            Parser.Advance();
            FunctionCodeBlock block = new FunctionCodeBlock
            {
                Name = name,
                IsAsynchronous = isAsync
            };
            if (Parser.Peek().Value != ")")
            {
                block.Parameters = ParseParams();
            }
            Parser.Accept(")");
            Parser.Advance();
            if (Parser.Peek().Value == "->")
            {
                Parser.Advance();
                block.ReturnHint = Parser.ParseExpression();
            }
            Parser.Accept(":");
            Parser.Advance();
            // TODO func_type_comment
            block.Statements = Parser.ParseBlock().Statements;
            return block;
        }
        //func_type_comment:
        //    | NEWLINE TYPE_COMMENT &(NEWLINE INDENT)   # Must be followed by indented block
        //    | TYPE_COMMENT
        // decorators: ('@' named_expression NEWLINE )+
        public List<Expression> ParseDecorators()
        {
            List<Expression> values = new List<Expression>();
            while (Parser.Peek().Value == "@")
            {
                Parser.Advance();
                values.Add(Parser.ParseNamedExpression());
                Parser.Accept("\n");
                Parser.Advance();
            }
            return values;
        }
        //params:
        //    | parameters
        public List<Expression> ParseParams()
        {
            return ParseParameters();
        }

        //parameters:
        //    | slash_no_default param_no_default* param_with_default*[star_etc] 
        //    | slash_with_default param_with_default* [star_etc] 
        //    | param_no_default+ param_with_default*[star_etc] 
        //    | param_with_default+ [star_etc] 
        //    | star_etc
        public List<Expression> ParseParameters()
        {
            throw new NotImplementedException();
        }

        //slash_no_default:
        //    | param_no_default+ '/' ',' 
        //    | param_no_default+ '/' &')' 
        //slash_with_default:
        //    | param_no_default* param_with_default+ '/' ',' 
        //    | param_no_default* param_with_default+ '/' &')' 

        //star_etc:
        //    | '*' param_no_default param_maybe_default* [kwds] 
        //    | '*' ',' param_maybe_default+ [kwds] 
        //    | kwds

        //kwds: '**' param_no_default
        public ParameterExpression ParseKwds()
        {
            Parser.Accept("**");
            Parser.Advance();
            ParameterExpression param = ParseParamNoDefault();
            param.DictionaryGenerator = true;
            return param;
        }
        //param_no_default:
        //    | param ',' TYPE_COMMENT? 
        //    | param TYPE_COMMENT? &')'
        public ParameterExpression ParseParamNoDefault()
        {
            // TODO TYPE_COMMENT?
            ParameterExpression param = ParseParam();
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance();
            }
            return param;
        }
        //param_with_default:
        //    | param default ',' TYPE_COMMENT? 
        //    | param default TYPE_COMMENT? &')'
        public ParameterExpression ParseParamWithDefault()
        {
            // TODO TYPE_COMMENT?
            ParameterExpression param = ParseParam();
            Parser.Accept("=");
            if (Parser.Peek().Value == "=")
            {
                param.Default = ParseDefault();
            }
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance();
            }
            return param;
        }
        //param_maybe_default:
        //    | param default? ',' TYPE_COMMENT? 
        //    | param default? TYPE_COMMENT? &')'
        public ParameterExpression ParseParamMaybeDefault()
        {
            // TODO TYPE_COMMENT?
            ParameterExpression param = ParseParam();
            if (Parser.Peek().Value == "=")
            {
                param.Default = ParseDefault();
            }
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance();
            }
            return param;
        }
        //param: NAME annotation?
        public ParameterExpression ParseParam()
        {
            string name = Parser.Peek().Value;
            Parser.Advance();
            ParameterExpression param = new ParameterExpression
            {
                Name = name
            };
            if (Parser.Peek().Value == ":")
            {
                param.Annotation = ParseAnnotation();
            }
            return param;
        }
        //annotation: ':' expression
        public Expression ParseAnnotation()
        {
            Parser.Accept(":");
            Parser.Advance();
            return Parser.ParseExpression();
        }
        //default: '=' expression
        public Expression ParseDefault()
        {
            Parser.Accept("=");
            Parser.Advance();
            return Parser.ParseExpression();
        }
    }
}
