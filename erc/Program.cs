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
            var baseName = "autotest";
            //var baseName = "example";

            var sourceFile = baseName + ".erc";
            var outputFile = baseName + ".out";

            var src = File.ReadAllText(sourceFile) + "\n\n\n" + ReadInternalLib();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

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

            var x64Generator = new WinX64CodeGenerator(true);
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

            var outStr = context.AST.ToTreeString() + "\n\n\n" + immediateCode + "\n\n\n" + nativeCode;
            //var outStr = nativeCode;
            Clipboard.SetText(outStr);
            File.WriteAllText("..\\..\\" + outputFile, outStr);

            //Compile .exe
            var folderName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var asmFileName = Path.Combine(folderName, baseName + ".asm");
            var exeFileName = Path.Combine(folderName, baseName + ".exe");
            File.WriteAllText(asmFileName, nativeCode);

            Console.WriteLine("Running FASM");
            Process.Start("cmd.exe", "/C cd /D \"" + folderName + "\" && del " + exeFileName + " && .\\fasmw\\fasm.exe " + asmFileName + " " + exeFileName + "");

            Console.WriteLine();
            Console.WriteLine("Compilation took: " + compilationTime + " ms");

            //Run .exe
            Console.WriteLine();
            Console.WriteLine("Running application");
            var runProcess = Process.Start("cmd.exe", "/C cd /D \"" + folderName + "\" && " + exeFileName + " && pause");
            runProcess.WaitForExit();
            Console.WriteLine("Application finished with code " + runProcess.ExitCode);

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
