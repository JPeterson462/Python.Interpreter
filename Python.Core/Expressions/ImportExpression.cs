using System;
using System.Collections.Generic;

namespace Python.Core.Expressions
{
    public class ImportExpression : Expression
    {
        /// <summary>
        /// List of import aliases. Imports[aliasName] = importPath
        /// </summary>
        public List<KeyValuePair<string, string>> Imports { get; set; }
    }
}
