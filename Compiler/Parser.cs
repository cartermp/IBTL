using Compiler.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Parser
    {
        /// <summary>
        /// Top-level parsing function.
        /// </summary>
        public static AST Parse(string fileName)
        {
            string contents = GetFileContents(fileName);
            // do some parsing yo

            AST ast = new AST();

            var lastToken = Lexer.GetToken(ref contents);

            return null;
        }

        /// <summary>
        /// Reads a file and outputs a string of its contents.
        /// </summary>
        private static string GetFileContents(string filename)
        {
            return string.Join("", File.ReadAllLines(filename).SelectMany(c => c));
        }
    }
}
