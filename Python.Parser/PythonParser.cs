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
        public PythonParser(List<Token> tokens) : base(tokens)
        {

        }
        // statements: statement+ 
        public Script Parse(int start = 0, int end = -1)
        {
            if (end < 0)
            {
                end = Tokens.Count;
            }
            Script script = new Script();

            return script;
        }
        // statement: compound_stmt | simple_stmts
        /*  simple_stmt:
                | assignment
                | star_expressions 
                | return_stmt
                | import_stmt
                | raise_stmt
                | 'pass' 
                | del_stmt
                | yield_stmt
                | assert_stmt
                | 'break' 
                | 'continue' 
                | global_stmt
                | nonlocal_stmt
            compound_stmt:
                | function_def
                | if_stmt
                | class_def
                | with_stmt
                | for_stmt
                | try_stmt
                | while_stmt
                | match_stmt
         */
        public Expression ParseStatement(int start, int end)
        {
            return null;
        }
        public Expression TryParseFunctionDefinition(int start, int end)
        {
            /*  function_def:
                    | decorators function_def_raw 
                    | function_def_raw

                function_def_raw:
                    | 'def' NAME '(' [params] ')' ['->' expression ] ':' [func_type_comment] block 
                    | ASYNC 'def' NAME '(' [params] ')' ['->' expression ] ':' [func_type_comment] block 
                func_type_comment:
                    | NEWLINE TYPE_COMMENT &(NEWLINE INDENT)   # Must be followed by indented block
                    | TYPE_COMMENT

                decorators: ('@' named_expression NEWLINE )+ 
             */
            Token token = Tokens[start];
            if (token.Type == TokenType.Decorator)
            {

            }
            return null;
        }
    }
}
