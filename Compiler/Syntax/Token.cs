using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Syntax
{
    /// <summary>
    /// Representation of an IBTL Token.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }
}
