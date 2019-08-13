using System;
using System.Collections.Generic;
using System.IO;

namespace erc
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = File.ReadAllText("example.erc");

            var context = new CompilerContext();
            context.Source = src;

            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(context);

            Console.WriteLine("TOKENS");
            Console.WriteLine("======");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            Console.WriteLine();
            Console.Write("Press Enter to close");
            Console.Read();
        }
    }
}
