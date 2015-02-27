using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    public class LexerException : Exception
    {
        public LexerException(string message, int line)
            : base(string.Format("Lexer error on line {0}: {1}", line, message)) { }
    }
}
