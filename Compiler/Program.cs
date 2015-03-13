using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputFile = "stutest.out";
            if (args.Any())
            {
                string contents = string.Empty;

                try
                {
                    string input = string.Join("", File.ReadAllLines(args.First()).SelectMany(c => c));
                    contents += new Parser().Parse(input).ToGforth();
                }
                catch (Exception ex)
                {
                    contents += ex.Message + "\n";
                }

                File.WriteAllText(outputFile, contents);

                return;
            }

            Console.WriteLine("Enter IBTL:");

            string ibtl = string.Empty;

            Console.Write("=> ");
            while (!string.IsNullOrWhiteSpace(ibtl = Console.ReadLine()))
            {
                try
                {
                    var parser = new Parser();
                    var tree = parser.Parse(ibtl);

                    tree.AddSymbols(parser.m_lexer.SymbolTable);

                    string gforth = tree.ToGforth();

                    Console.WriteLine("Equivalent Gforth:\n");
                    Console.WriteLine(gforth);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("=> ");
            }
        }
    }
}
