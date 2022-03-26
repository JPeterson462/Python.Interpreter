using System;
using System.Collections.Generic;
using Python.Core;
using Python.Core.CodeBlocks;
using Python.Core.Expressions;

namespace Python.Parser
{
    public class LambdaSubParser
    {
        public PythonParser Parser { get; set; }
        public LambdaSubParser(PythonParser parser)
        {
            Parser = parser;
        }
        //lambdef:
        // | 'lambda' [lambda_params] ':' expression
        public Expression ParseLambdef()
        {
            Parser.Accept(Keyword.Lambda.Value);
            Parser.Advance();
            CollectionExpression parameters = ParseLambdaParams();
            Parser.Accept(":");
            Parser.Advance();
            return new FunctionCodeBlock
            {
                Parameters = parameters,
                Statements = new List<Expression>
                {
                    Parser.ParseExpression()
                }
            };
        }
        // lambda_params:
        // | lambda_parameters
        public CollectionExpression ParseLambdaParams()
        {
            return ParseLambdaParameters();
        }
        // 
        // # lambda_parameters etc. duplicates parameters but without annotations
        // # or type comments, and if there's no comma after a parameter, we expect
        // # a colon, not a close parenthesis.  (For more, see parameters above.)
        // #
        //     lambda_parameters:
        //     | lambda_slash_no_default lambda_param_no_default* lambda_param_with_default*[lambda_star_etc] 
        //     | lambda_slash_with_default lambda_param_with_default* [lambda_star_etc] 
        //     | lambda_param_no_default+ lambda_param_with_default*[lambda_star_etc] 
        //     | lambda_param_with_default+ [lambda_star_etc] 
        //     | lambda_star_etc
        public CollectionExpression ParseLambdaParameters()
        {
            if ((Parser.IndexOf("/") < Parser.IndexOf(",") || Parser.IndexOf("/") < Parser.IndexOf(":"))
                && Parser.Peek(1).Value != "=" && Parser.IndexOf("/") >= 0)
            {
                List<Expression> parameters = new List<Expression>();
                // lambda_slash_no_default lambda_param_no_default* lambda_param_with_default* [lambda_star_etc] 
                CollectionExpression slashNoDefault = ParseLambdaSlashNoDefault();
                parameters.AddRange(slashNoDefault.Elements);
                while (Parser.Peek(1).Value != "=")
                {
                    parameters.Add(ParseLambdaParamNoDefault());
                }
                while (Parser.Peek(1).Value == "=")
                {
                    parameters.Add(ParseLambdaParamWithDefault());
                }
                if (Parser.Peek().Value == "*" || Parser.Peek().Value == "**")
                {
                    parameters.AddRange(ParseLambdaStarEtc().Elements);
                }
                return new CollectionExpression
                {
                    Elements = parameters,
                    Type = CollectionType.Tuple
                };
            }
            if ((Parser.IndexOf("/") < Parser.IndexOf(",") || Parser.IndexOf("/") < Parser.IndexOf(":"))
                && Parser.Peek(1).Value == "=" && Parser.IndexOf("/") >= 0)
            {
                List<Expression> parameters = new List<Expression>();
                // lambda_slash_with_default lambda_param_with_default* [lambda_star_etc] 
                CollectionExpression slashWithDefault = ParseLambdaSlashWithDefault();
                parameters.AddRange(slashWithDefault.Elements);
                while (Parser.Peek(1).Value == "=")
                {
                    parameters.Add(ParseLambdaParamWithDefault());
                }
                if (Parser.Peek().Value == "*" || Parser.Peek().Value == "**")
                {
                    parameters.AddRange(ParseLambdaStarEtc().Elements);
                }
                return new CollectionExpression
                {
                    Elements = parameters,
                    Type = CollectionType.Tuple
                };
            }
            if (Parser.Peek(1).Value != "=")
            {
                List<Expression> parameters = new List<Expression>();
                // lambda_param_no_default+ lambda_param_with_default*[lambda_star_etc] 
                Expression ex = ParseLambdaParamNoDefault();
                parameters.Add(ex);
                while (Parser.Peek(1).Value == "=")
                {
                    parameters.Add(ParseLambdaParamWithDefault());
                }
                if (Parser.Peek().Value == "*" || Parser.Peek().Value == "**")
                {
                    parameters.AddRange(ParseLambdaStarEtc().Elements);
                }
                return new CollectionExpression
                {
                    Elements = parameters,
                    Type = CollectionType.Tuple
                };
            }
            else if (Parser.Peek().Value != "*" && Parser.Peek().Value != "**")
            {
                List<Expression> parameters = new List<Expression>();
                // lambda_param_with_default+ [lambda_star_etc] 
                while (Parser.Peek(1).Value == "=")
                {
                    parameters.Add(ParseLambdaParamWithDefault());
                }
                if (Parser.Peek().Value == "*" || Parser.Peek().Value == "**")
                {
                    parameters.AddRange(ParseLambdaStarEtc().Elements);
                }
                return new CollectionExpression
                {
                    Elements = parameters,
                    Type = CollectionType.Tuple
                };
            }
            else
            {
                return ParseLambdaStarEtc();
            }
        }

        //     lambda_slash_no_default:
        //     | lambda_param_no_default+ '/' ',' 
        //     | lambda_param_no_default+ '/' &':'
        public CollectionExpression ParseLambdaSlashNoDefault()
        {
            List<Expression> values = new List<Expression>();
            while (Parser.Peek().Value != "/" && Parser.Peek().Value != ":")
            {
                values.Add(ParseLambdaParamNoDefault());
            }
            Parser.Accept("/");
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance(); // consume the comma
            }
            return new CollectionExpression
            {
                Type = CollectionType.Tuple,
                Elements = values
            };
        }
        //     lambda_slash_with_default:
        //     | lambda_param_no_default* lambda_param_with_default+ '/' ',' 
        //     | lambda_param_no_default* lambda_param_with_default+ '/' &':'
        public CollectionExpression ParseLambdaSlashWithDefault()
        {
            List<Expression> values = new List<Expression>();
            while (Parser.Peek(1).Value == ",")
            {
                values.Add(ParseLambdaParamNoDefault());
            }
            while (Parser.Peek().Value != "/" && Parser.Peek().Value != ":")
            {
                values.Add(ParseLambdaParamWithDefault());
            }
            Parser.Accept("/");
            if (Parser.Peek().Value == ",")
            {
                Parser.Advance(); // consume the comma
            }
            return new CollectionExpression
            {
                Type = CollectionType.Tuple,
                Elements = values
            };
        }

        //     lambda_star_etc:
        //     | '*' lambda_param_no_default lambda_param_maybe_default* [lambda_kwds] 
        //     | '*' ',' lambda_param_maybe_default+ [lambda_kwds] 
        //     | lambda_kwds
        public CollectionExpression ParseLambdaStarEtc()
        {
            if (Parser.Peek().Value == "*")
            {
                Parser.Advance();
                if (Parser.Peek().Value != ",")
                {
                    List<Expression> values = new List<Expression>();
                    values.Add(ParseLambdaParamNoDefault());
                    while (Parser.Peek().Type == TokenType.Variable)
                    {
                        values.Add(ParseLambdaParamMaybeDefault());
                    }
                    if (Parser.Peek().Value == "**")
                    {
                        values.Add(ParseLambdaKeywords());
                    }
                    return new CollectionExpression
                    {
                        Type = CollectionType.Tuple,
                        Elements = values
                    };
                }
                else
                {
                    Parser.Advance(); // consume the comma
                    List<Expression> values = new List<Expression>();
                    while (Parser.Peek().Type == TokenType.Variable)
                    {
                        values.Add(ParseLambdaParamMaybeDefault());
                    }
                    if (Parser.Peek().Value == "**")
                    {
                        values.Add(ParseLambdaKeywords());
                    }
                    return new CollectionExpression
                    {
                        Type = CollectionType.Tuple,
                        Elements = values
                    };
                }
            }
            else
            {
                return ParseLambdaKeywords();
            }
        }

        //     lambda_kwds: '**' lambda_param_no_default
        public CollectionExpression ParseLambdaKeywords()
        {
            Parser.Accept("**");
            Parser.Advance();
            return new CollectionExpression
            {
                Elements = new List<Expression> {
                    new OperatorExpression {
                        Expression = ParseLambdaParamNoDefault(),
                        Operator = Operator.Exponentiation
                    }
                }
            };
        }

        //     lambda_param_no_default:
        //     | lambda_param ',' 
        //     | lambda_param &':'
        public Expression ParseLambdaParamNoDefault()
        {
            Expression param = ParseLambdaParam();
            if (Parser.Peek().Value != ":")
            {
                Parser.Accept(",");
            }
            if (Parser.Peek().Value == ",")
            {
                // consume the comma
                Parser.Advance();
            }
            return new LambdaParameterExpression
            {
                Identifier = param,
                Default = null
            };
        }
        //     lambda_param_with_default:
        //     | lambda_param default ',' 
        //     | lambda_param default &':'
        public Expression ParseLambdaParamWithDefault()
        {
            Expression param = ParseLambdaParam();
            Expression defaultval = Parser.ParseDefault();
            if (Parser.Peek().Value != ":")
            {
                Parser.Accept(",");
            }
            if (Parser.Peek().Value == ",")
            {
                // consume the comma
                Parser.Advance();
            }
            return new LambdaParameterExpression
            {
                Identifier = param,
                Default = defaultval
            };
        }
        //     lambda_param_maybe_default:
        //     | lambda_param default? ',' 
        //     | lambda_param default? &':'
        public Expression ParseLambdaParamMaybeDefault()
        {
            Expression param = ParseLambdaParam();
            Expression defaultval = null;
            if (Parser.Peek().Value == "=")
            {
                defaultval = Parser.ParseDefault();
            }
            if (Parser.Peek().Value != ":")
            {
                Parser.Accept(",");
            }
            if (Parser.Peek().Value == ",")
            {
                // consume the comma
                Parser.Advance();
            }
            return new LambdaParameterExpression
            {
                Identifier = param,
                Default = defaultval
            };
        }
        
        //         lambda_param: NAME
        public Expression ParseLambdaParam()
        {
            string name = Parser.Peek().Value;
            Parser.Advance();
            return new SimpleExpression
            {
                IsVariable = true, IsConstant = false,
                Value = name
            };
        }
    }
}
