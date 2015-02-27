using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Syntax
{
    /// <summary>
    /// Represents the various types of tokens IBTL supports.
    /// </summary>
    public enum TokenType
    {
        LeftParenthesis,
        RightParenthesis,
        
        Assignment,
        BinaryOperator,
        UnaryOperator,
        RelationalOperator,

        Int,
        Real,
        String,
        True,
        False,

        Statement,
        Type,
        Identifier
    }
}
