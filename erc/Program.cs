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
            tokenizer.Tokenize(context);

            Console.WriteLine("TOKENS");
            Console.WriteLine("======");
            foreach (var token in context.Tokens)
            {
                Console.WriteLine(token);
            }

            var syntax = new Syntax();
            syntax.Analyze(context);

            Console.WriteLine();
            Console.WriteLine("STATEMENTS");
            Console.WriteLine("==========");
            foreach (var statement in context.Statements)
            {
                Console.WriteLine(statement);
            }

            Console.WriteLine();
            Console.Write("Press Enter to close");
            Console.Read();
        }
    }
}
