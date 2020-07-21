using System;
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

            var imGenerator = new IMCodeGenerator();
            imGenerator.Generate(context);

            var x64Generator = new WinX64CodeGenerator();
            x64Generator.Generate(context);

            stopWatch.Stop();
            var compilationTime = stopWatch.ElapsedMilliseconds;

            var immediateCode = String.Join("\n", context.IMObjects);
            var nativeCode = String.Join("\n", context.NativeCode);
            
            Console.WriteLine();
            Console.WriteLine("AST");
            Console.WriteLine("===");
            Console.Write(context.AST.ToTreeString());

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("IM CODE");
            Console.WriteLine("==========");
            Console.WriteLine(immediateCode);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("NATIVE CODE");
            Console.WriteLine("==========");
            Console.WriteLine(nativeCode);

            Clipboard.SetText(context.AST.ToTreeString() + "\n\n\n" + immediateCode + "\n\n\n" + nativeCode);

            Console.WriteLine();
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
