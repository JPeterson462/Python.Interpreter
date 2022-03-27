using System;
using Python.Core;
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
            //    | function_def
            //    | if_stmt
            //    | class_def
            //    | with_stmt
            //    | for_stmt
            //    | try_stmt
            //    | while_stmt
            //    | match_stmt
            throw new NotImplementedException();
        }
    }
}
