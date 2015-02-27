using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Syntax
{
    /// <summary>
    /// Represents a token for Semantic Analysis.  Although this is still
    /// the same type as a regular Token, this is used specifically for Code Generation.
    /// </summary>
    public class SemanticToken
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }
}
