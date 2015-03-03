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

                foreach (string file in args)
                {
                    try
                    {
                        contents += File.ReadAllLines(file).SelectMany(c => c).ToString();
                    } catch (Exception ex)
                    {
                        contents += ex.Message;
                    }
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
                    string gforth = new Parser().Parse(ibtl).ToGforth();
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
