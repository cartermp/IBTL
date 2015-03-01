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
            Console.WriteLine("Enter IBTL:");

            string ibtl = string.Empty;

            while (!string.IsNullOrWhiteSpace(ibtl = Console.ReadLine()))
            {
                string gforth = new Parser().Parse(ibtl).ToGforth();
                Console.WriteLine("Equivalent Gforth:\n");
                Console.WriteLine(gforth);
            }
        }
    }
}
