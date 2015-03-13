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
        Undefined,
        // Temporary type to handle turning into unary or binary at parse time.
        Minus,

        LeftParenthesis,
        RightParenthesis,
        
        Assignment,
        BinaryOperator,
        UnaryOperator,

        Int,
        Real,
        String,
        True,
        False,
        Boolean,

        Statement,
        Type,
        Identifier
    }
}
