using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngouriMath.Terminal
{
    public sealed class UserInterface
    {
        private static string Time
            => DateTime.Now.ToString("HH:mm:ss");

        public UserInterface()
        {
            Console.Title = "AngouriMath Terminal.";
        }

        private static void PrintPrefix(string prefix)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"\n{prefix}[{Time}] = ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public string ReadLine()
        {
            PrintPrefix("In");
            return Console.ReadLine() ?? throw new Exception();
        }
        public void WriteLine(string input)
        {
            PrintPrefix("Out");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(input);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public void WriteLineError(string input)
        {
            PrintPrefix("Error");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(input);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
