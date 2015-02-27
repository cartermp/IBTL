using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var file in string.Join(" ", args).Split(' '))
            {
                var ast = Parser.Parse(file);
                string gforth = ast.ToGforth();
            }
        }
    }
}
