using System;
using System.Collections.Generic;
using System.Linq;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    /// <summary>
    /// Recursive descent parser
    /// </summary>
    public class PythonParser : Parser
    {
        public OperationSubParser OperationSubParser { get; set; }
        public AtomSubParser AtomSubParser { get; set; }
        public LambdaSubParser LambdaSubParser { get; set; }
        public OperatorSubParser OperatorSubParser { get; set; }
        public ArgumentsSubParser ArgumentsSubParser { get; set; }
        public CompoundSubParser CompoundSubParser { get; set; }
        public PythonParser(List<Token> tokens) : base(tokens)
        {
            OperationSubParser = new OperationSubParser(this);
            AtomSubParser = new AtomSubParser(this);
            LambdaSubParser = new LambdaSubParser(this);
            OperatorSubParser = new OperatorSubParser(this);
            ArgumentsSubParser = new ArgumentsSubParser(this);
            CompoundSubParser = new CompoundSubParser(this);
        }
        // statements: statement+ 
        public Script Parse()
        {
            Script script = new Script();
            while (Position < Tokens.Count)
            {
                script.Statements.Add(ParseStatement());
            }
            return script;
        }
        // statements: statement+
        public CodeBlock ParseStatements(TokenType endToken)
        {
            // we need an endToken to handle blocks
            CodeBlock block = new CodeBlock();
            while (Peek().Type != endToken)
            {
                block.Statements.Add(ParseStatement());
            }
            return block;
        }
        // statement: compound_stmt  | simple_stmts
        public Expression ParseStatement()
        {
            Token token = Peek();
           /* // compound_stmt
            if (token.Type == TokenType.Decorator)
            {
                throw new NotImplementedException();
            } */
           // FIXME
            return ParseSimpleStmts();
        }
        // simple_stmts:
        //    | simple_stmt !';' NEWLINE  # Not needed, there for speedup
        //    | ';'.simple_stmt+ [';'] NEWLINE
        public CodeBlock ParseSimpleStmts()
        {
            CodeBlock block = new CodeBlock();
            Expression stmt = OperatorSubParser.ParseSimpleStmt();
            block.Statements.Add(stmt);
            while (Peek().Value == ";")
            {
                Advance();
                block.Statements.Add(OperatorSubParser.ParseSimpleStmt());
            }
            Accept("\n");
            Advance();
            return block;
        }
        // decorators: ('@' named_expression NEWLINE )+ 
        public Expression ParseDecorator()
        {
            Accept(TokenType.Decorator);
            Advance(1);
            Expression ex = ParseNamedExpression();
            Accept(TokenType.EndOfExpression);
            return ex;
        }
        /*
            named_expression:
            | assignment_expression
            | expression !':='
         */
        //  assignment_expression:
        //  | NAME ':=' ~expression
        public Expression ParseNamedExpression()
        {
            if (Peek(1).Value == Operator.Assignment.Value)
            {
                return ParseAssignmentExpression();
            }
            else
            {
                return ParseExpression();
            }
        }
        public Expression ParseAssignmentExpression()
        {
            Token token = Peek();
            string name = token.Value;
            Advance();
            Accept(Operator.Assignment.Value);
            Advance();
            Expression ex = ParseExpression();
            return new EvaluatedExpression
            {
                LeftHandValue = new SimpleExpression
                {
                    Value = name,
                    IsVariable = true
                },
                Operator = Operator.Assignment,
                RightHandValue = ex
            };
        }
        // star_expressions:
        // | star_expression(',' star_expression )+ [','] 
        // | star_expression ',' 
        // | star_expression
        public Expression ParseStarExpressions()
        {
            Expression expression = ParseStarExpression();
            List<Expression> expressions = new List<Expression>();
            expressions.Add(expression);
            while (Peek().Value == "," || Peek().Type == TokenType.ElementSeparator)
            {
                Advance();
                expressions.Add(ParseStarExpression());
            }
            return new CollectionExpression
            {
                Elements = expressions,
                Type = CollectionType.Unknown
            };
        }
        //star_expression:
        // | '*' bitwise_or 
        // | expression
        public Expression ParseStarExpression()
        {
            if (Peek().Value == Operator.Multiply.Value)
            {
                Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.Multiply,
                    RightHandValue = OperationSubParser.ParseBitwiseOr()
                };
            }
            return ParseExpression();
        }
        //expression:
        //| disjunction 'if' disjunction 'else' expression 
        //| disjunction
        //| lambdef
        public Expression ParseExpression()
        {
            if (Peek().Value == Keyword.Lambda.Value)
            {
                return LambdaSubParser.ParseLambdef();
            }
            else
            {
                Expression expression = OperationSubParser.ParseDisjunction();
                if (Peek().Value == Keyword.If.Value)
                {
                    Advance();
                    Expression condition = OperationSubParser.ParseDisjunction();
                    Accept(Keyword.Else.Value);
                    Advance();
                    return new TernaryExpression
                    {
                        Condition = condition,
                        TrueCase = expression,
                        FalseCase = ParseExpression()
                    };
                }
                return expression;
            }
        }
        public Expression ParseStarNamedExpression()
        {
            if (Peek().Value == Operator.Multiply.Value)
            {
                Advance();
                return new EvaluatedExpression
                {
                    LeftHandValue = null,
                    Operator = Operator.Multiply,
                    RightHandValue = OperationSubParser.ParseBitwiseOr()
                };
            }
            else
            {
                return ParseNamedExpression();
            }
        }
        // default: '=' expression
        public Expression ParseDefault()
        {
            Accept("=");
            Advance();
            return ParseExpression();
        }
        // block:
        //    | NEWLINE INDENT statements DEDENT 
        //    | simple_stmts
        public CodeBlock ParseBlock()
        {
            if (Peek().Value == "\n")
            {
                Accept("\n");
                Advance();
                Accept(TokenType.IndentTab);
                Advance();
                CodeBlock block = ParseStatements(TokenType.DedentTab);
                Accept(TokenType.DedentTab);
                Advance();
                return block;
            }
            else
            {
                return ParseSimpleStmts();
            }
        }
    }
}
