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
        public PythonParser(List<Token> tokens) : base(tokens)
        {
            OperationSubParser = new OperationSubParser(this);
            AtomSubParser = new AtomSubParser(this);
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
        //expression:
        //| disjunction 'if' disjunction 'else' expression 
        //| disjunction
        //| lambdef
        public Expression ParseExpression()
        {

            return null;
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
    }
}
