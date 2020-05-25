﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace erc
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var src = File.ReadAllText("example.erc") + "\n\n\n" + ReadInternalLib();

            var context = new CompilerContext();
            context.Source = src;

            var tokenizer = new Tokenizer();
            tokenizer.Tokenize(context);

            /*Console.WriteLine("TOKENS");
            Console.WriteLine("======");
            foreach (var token in context.Tokens)
            {
                Console.WriteLine(token);
            }*/

            var syntax = new SyntaxAnalysis();
            syntax.Analyze(context);

            /*Console.WriteLine();
            Console.WriteLine("AST");
            Console.WriteLine("===");
            Console.Write(context.AST.ToTreeString());*/

            var semantic = new SemanticAnalysis();
            semantic.Analyze(context);

            var processor = new PostProcessor();
            processor.Process(context);

            var imGenerator = new IMCodeGenerator();
            imGenerator.Generate(context);

            var x64Generator = new WinX64CodeGenerator();
            x64Generator.Generate(context);

            string finalCode = String.Join("\n", context.IMObjects);

            stopWatch.Stop();
            var compilationTime = stopWatch.ElapsedMilliseconds;
            
            Console.WriteLine();
            Console.WriteLine("AST");
            Console.WriteLine("===");
            Console.Write(context.AST.ToTreeString());

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("CODE");
            Console.WriteLine("==========");
            Console.WriteLine(finalCode);

            Clipboard.SetText(finalCode);

            Console.WriteLine("Compilation took: " + compilationTime + " ms");

            Console.WriteLine();
            Console.Write("Press Enter to close");
            Console.Read();
        }

        private static string ReadInternalLib()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("erc.internal_lib.erc");
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

    }
}
