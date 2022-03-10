using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class AtomSubParser
    {
        public PythonParser Parser { get; set; }
        public AtomSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        public Expression ParseForIfClause()
        {
            bool isAsync = false;
            if (Parser.Peek().Value == Keyword.Async.Value)
            {
                Parser.Advance();
                isAsync = true;
            }
            Parser.Accept(Keyword.For.Value);

            throw new NotImplementedException();
        }
        public Expression ParseAtom()
        {
            Token token = Parser.Peek();
            if (token.Value == Keyword.True.Value || token.Value == Keyword.False.Value ||
                token.Value == Keyword.None.Value || token.Type == TokenType.Number ||
                token.Type == TokenType.String || token.Type == TokenType.Variable)
            {
                Parser.Advance();
                return new SimpleExpression
                {
                    Value = token.Value,
                    IsConstant = token.Type == TokenType.String || token.Type == TokenType.Number ||
                                token.Value == Keyword.True.Value || token.Value == Keyword.False.Value ||
                                token.Value == Keyword.None.Value,
                    IsVariable = token.Type == TokenType.Variable
                };
            }
            else
            {
                /*
                    list:
                        | '[' [star_named_expressions] ']' 
                    listcomp:
                        | '[' named_expression for_if_clauses ']' 
                    tuple:
                        | '(' [star_named_expression ',' [star_named_expressions]  ] ')' 
                    group:
                        | '(' (yield_expr | named_expression) ')' 
                    genexp:
                        | '(' ( assignment_expression | expression !':=') for_if_clauses ')' 
                    set: '{' star_named_expressions '}' 
                    setcomp:
                        | '{' named_expression for_if_clauses '}' 
                    dict:
                        | '{' [double_starred_kvpairs] '}' 

                    dictcomp:
                        | '{' kvpair for_if_clauses '}' 
                 */
                if(token.Type == TokenType.BeginParameters)
                {
                    Parser.Advance();

                    Parser.Accept(TokenType.EndParameters);
                    Parser.Advance();
                }
                else if (token.Type == TokenType.BeginList)
                {
                    Parser.Advance();
                    CollectionExpression collection = new CollectionExpression
                    {
                        Elements = new List<Expression>()
                    };
                    Expression element = Parser.ParseStarNamedExpression();
                    if (Parser.Peek().Value == Keyword.Async.Value || Parser.Peek().Value == Keyword.For.Value)
                    {

                    }
                    else
                    {
                        collection.Type = CollectionType.List;
                        collection.Elements.Add(element);
                        while (Parser.Peek().Type != TokenType.EndList)
                        {
                            Parser.Accept(TokenType.ElementSeparator);
                            Parser.Advance();
                            collection.Elements.Add(Parser.ParseStarNamedExpression());
                        }
                    }
                    Parser.Accept(TokenType.EndList);
                    Parser.Advance();
                    return collection;
                }
                else if (token.Type == TokenType.DictionaryStart)
                {
                    Parser.Advance();

                    Parser.Accept(TokenType.DictionaryEnd);
                    Parser.Advance();
                }
                else
                {
                    Parser.ThrowSyntaxError(Parser.Position);
                }
            }


            throw new NotImplementedException();
        }

    }
}
