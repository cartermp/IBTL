using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    public class SemanticException : Exception
    {
        public SemanticException(string message)
            : base("Semantic Error: " + message) { }
    }
}
