using System;
using System.Collections.Generic;
using Python.Core;

namespace Python.Parser
{
    public class ToplevelSubParser
    {
        public PythonParser Parser { get; set; }
        public ToplevelSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //file: [statements] ENDMARKER
        public List<Expression> ParseFile()
        {
            List<Expression> expressions = ParseExpressions();
            Parser.Accept(TokenType.EndOfExpression);
            Parser.Advance();
            if (Parser.Errors.Count > 0)
            {
                throw new Exception("Syntax error!");
            }
            return expressions;
        }
        //interactive: statement_newline
        public List<Expression> ParseInteractive()
        {
            return ParseStatementNewline();
        }
        //eval: expressions NEWLINE* ENDMARKER
        public List<Expression> ParseEval()
        {
            List<Expression> expressions = ParseExpressions();
            while (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == "\n")
            {
                Parser.Advance(); // just consume
            }
            Parser.Accept(TokenType.EndOfExpression);
            Parser.Advance();
            if (Parser.Errors.Count > 0)
            {
                throw new Exception("Syntax error!");
            }
            return expressions;
        }

        // TODO: where in the grammar are these used? (func_type, fstring, and type_expressions)

        //func_type: '(' [type_expressions] ')' '->' expression NEWLINE* ENDMARKER
        //fstring: star_expressions

        //# type_expressions allow */** but ignore them
        //type_expressions:
        //    | ','.expression+ ',' '*' expression ',' '**' expression 
        //    | ','.expression+ ',' '*' expression 
        //    | ','.expression+ ',' '**' expression 
        //    | '*' expression ',' '**' expression //
        //    | '*' expression 
        //    | '**' expression 
        //    | ','.expression+ 

        //statement_newline:
        //    | compound_stmt NEWLINE 
        //    | simple_stmts
        //    | NEWLINE 
        //    | ENDMARKER
        public List<Expression> ParseStatementNewline()
        {
            List<Expression> expressions = new List<Expression>();
            Expression compound = Parser.CompoundSubParser.ParseCompoundStatement();
            if (compound != null)
            {
                Parser.Accept("\n");
                Parser.Advance();
                expressions.Add(compound);
            }
            else
            {
                if (Parser.Peek().Value == "\n" || Parser.Peek().Type == TokenType.EndOfExpression)
                {
                    // empty
                }
                else
                {
                    expressions.AddRange(Parser.ParseSimpleStmts().Statements);
                }
            }
            if (Parser.Errors.Count > 0)
            {
                throw new Exception("Syntax error!");
            }
            return expressions;
        }

        //expressions:
        //    | expression(',' expression )+ [','] 
        //    | expression ',' 
        //    | expression
        public List<Expression> ParseExpressions()
        {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(Parser.ParseExpression());
            while (Parser.Peek().Value == ",")
            {
                Parser.Advance();
                expressions.Add(Parser.ParseExpression());
            }
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance();
            }
            if (Parser.Errors.Count > 0)
            {
                throw new Exception("Syntax error!");
            }
            return expressions;
        }
    }
}
