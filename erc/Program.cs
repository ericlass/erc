using System;
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

            var processor = new PostProcessor();
            processor.Process(context);

            Console.WriteLine();
            Console.WriteLine("STORAGE");
            Console.WriteLine("=======");
            var locator = new StorageLocator();
            locator.Locate(context);

            Console.WriteLine();
            Console.WriteLine("AST");
            Console.WriteLine("===");
            Console.Write(context.AST.ToTreeString());

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("CODE");
            Console.WriteLine("==========");
            var generator = new CodeGenerator();
            Console.WriteLine(generator.Generate(context));

            Console.WriteLine();
            Console.Write("Press Enter to close");
            Console.Read();
        }
    }
}
