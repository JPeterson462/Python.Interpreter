using System;
using Python.Core;
using Python.Core.Expressions;
namespace Python.Parser
{
    public class OperatorSubParser
    {
        public PythonParser Parser { get; set; }
        public OperatorSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //simple_stmt:
        //    | assignment
        //    | star_expressions 
        //    | return_stmt
        //    | import_stmt
        //    | raise_stmt
        //    | 'pass' 
        //    | del_stmt
        //    | yield_stmt
        //    | assert_stmt
        //    | 'break' 
        //    | 'continue' 
        //    | global_stmt
        //    | nonlocal_stmt
        public Expression ParseSimpleStmt()
        {
            string next = Parser.Peek().Value;
            if (next == Keyword.Nonlocal.Value)
            {
                Parser.Advance();
                // NAME should be one token
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(new SimpleExpression
                {
                    IsConstant = false,
                    IsVariable = true,
                    Value = Parser.Peek().Value
                });
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(new SimpleExpression
                    {
                        IsConstant = false,
                        IsVariable = true,
                        Value = Parser.Peek().Value
                    });
                    Parser.Advance();
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Nonlocal,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Global.Value)
            {
                Parser.Advance();
                // NAME should be one token
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(new SimpleExpression
                {
                    IsConstant = false,
                    IsVariable = true,
                    Value = Parser.Peek().Value
                });
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(new SimpleExpression
                    {
                        IsConstant = false,
                        IsVariable = true,
                        Value = Parser.Peek().Value
                    });
                    Parser.Advance();
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Global,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Continue.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Continue,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Break.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Break,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Assert.Value)
            {
                Parser.Advance();
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(Parser.ParseExpression());
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(Parser.ParseExpression());
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Assert,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Yield.Value)
            {
                return Parser.AtomSubParser.ParseYieldExpr();
            }
            // 'del' del_targets &(';' | NEWLINE) 
            if (next == Keyword.Del.Value)
            {
                Parser.Advance();
                CollectionExpression ex = new CollectionExpression
                {
                    Type = CollectionType.Unknown
                };
                ex.Elements.Add(ParseDelTarget());
                while (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    ex.Elements.Add(ParseDelTarget());
                }
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Del,
                    Operator = null,
                    Expression = ex
                };
            }
            if (next == Keyword.Pass.Value)
            {
                Parser.Advance();
                return new OperatorExpression
                {
                    KeywordOperator = Keyword.Pass,
                    Operator = null,
                    Expression = null
                };
            }
            if (next == Keyword.Raise.Value)
            {
                Parser.Advance();
                if (Parser.Peek().Type != TokenType.EndOfExpression)
                {
                    Expression ex = Parser.ParseExpression();
                    if (Parser.Peek().Value == Keyword.From.Value)
                    {
                        Parser.Advance();
                        Expression src = Parser.ParseExpression();
                        return new RaiseExpression
                        {
                            Expression = ex,
                            Source = src
                        };
                    }
                    else
                    {
                        return new RaiseExpression
                        {
                            Expression = ex,
                            Source = null
                        };
                    }
                }
                else
                {
                    return new RaiseExpression();
                }
            }
            //| import_stmt
            //| return_stmt
            //| star_expressions
            //assignment

            throw new NotImplementedException();
        }
        // del_targets: ','.del_target+ [','] 
        //  del_target:
        //   | t_primary '.' NAME !t_lookahead 
        //   | t_primary '[' slices ']' !t_lookahead 
        //   | del_t_atom
        // del_t_atom:
        //   | NAME 
        //   | '(' del_target ')' 
        //   | '(' [del_targets] ')' 
        //   | '[' [del_targets] ']'
        public Expression ParseDelTarget()
        {
            // FIXME
            throw new NotImplementedException();
        }
    }
}
