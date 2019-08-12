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

            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(src);

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
