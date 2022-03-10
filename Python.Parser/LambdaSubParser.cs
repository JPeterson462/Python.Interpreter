using System;
using Python.Core;
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

            throw new NotImplementedException();
        }
        // lambda_params:
        // | lambda_parameters
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

        //     lambda_slash_no_default:
        //     | lambda_param_no_default+ '/' ',' 
        //     | lambda_param_no_default+ '/' &':' 
        //     lambda_slash_with_default:
        //     | lambda_param_no_default* lambda_param_with_default+ '/' ',' 
        //     | lambda_param_no_default* lambda_param_with_default+ '/' &':' 

        //     lambda_star_etc:
        //     | '*' lambda_param_no_default lambda_param_maybe_default* [lambda_kwds] 
        //     | '*' ',' lambda_param_maybe_default+ [lambda_kwds] 
        //     | lambda_kwds
        public Expression ParseLambdaStarEtc()
        {
            if (Parser.Peek().Value == "*")
            {
                Parser.Advance();
                if (Parser.Peek().Value == ",")
                {
                    Parser.Advance();
                    Expression firstParam = ParseLambdaParamNoDefault();
                    
                }
                else
                {

                }
                throw new NotImplementedException();
            }
            else
            {
                return ParseLambdaKeywords();
            }
        }

        //     lambda_kwds: '**' lambda_param_no_default
        public Expression ParseLambdaKeywords()
        {
            Parser.Accept("**");
            Parser.Advance();
            return new EvaluatedExpression
            {
                LeftHandValue = null,
                Operator = Operator.Exponentiation, // TODO need special handling
                RightHandValue = ParseLambdaParamNoDefault()
            };
        }

        //     lambda_param_no_default:
        //     | lambda_param ',' 
        //     | lambda_param &':'
        public Expression ParseLambdaParamNoDefault()
        {
            Expression param = ParseLambdaParam();
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
