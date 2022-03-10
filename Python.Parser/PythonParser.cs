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
        public PythonParser(List<Token> tokens) : base(tokens)
        {
            OperationSubParser = new OperationSubParser(this);
            AtomSubParser = new AtomSubParser(this);
            LambdaSubParser = new LambdaSubParser(this);
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
        // statement: compound_stmt  | simple_stmts
        public Expression ParseStatement()
        {
            Token token = Peek();
            // compound_stmt
            if (token.Type == TokenType.Decorator)
            {

            }
            return null;
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
            while (Peek().Value == ",")
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
    }
}
