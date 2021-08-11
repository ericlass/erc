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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("erc compiler - alpha");
            Console.WriteLine("--------------------");

            //var baseName = "autotest";
            var baseName = "example";

            var config = Config.Load();

            var sourceFile = baseName + ".erc";
            var outputFile = baseName + ".out";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>> Compiling: " + sourceFile);

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
            Console.WriteLine(">>> Compilation took: " + compilationTime + " ms");

            var immediateCode = String.Join("\n", context.IMObjects);
            var nativeCode = String.Join("\n", context.NativeCode);

            var outStr = context.AST.ToTreeString() + "\n\n\n" + immediateCode + "\n\n\n" + nativeCode;
            //var outStr = nativeCode;
            Clipboard.SetText(outStr);
            File.WriteAllText("..\\..\\" + outputFile, outStr);

            /*Console.WriteLine();
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
            Console.WriteLine(nativeCode);*/

            //Compile .exe
            var folderName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var exeFileName = baseName + ".exe";
            var exeFilePath = Path.Combine(folderName, exeFileName);
            var asmFilePath = Path.Combine(folderName, baseName + ".asm");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>> Running FASM");
            Console.ForegroundColor = ConsoleColor.Yellow;
            AssembleExecutable(config, nativeCode, exeFilePath, asmFilePath);

            //Run compiled .exe
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>> Running " + exeFileName);
            Console.ResetColor();

            var appStartInfo = new ProcessStartInfo();
            appStartInfo.FileName = "cmd.exe";
            appStartInfo.Arguments = "/C " + exeFilePath + " & pause";
            var appProcess = Process.Start(appStartInfo);
            appProcess.WaitForExit();

            Console.WriteLine();
            Console.Write("Press Enter to close");
            Console.Read();
        }

        private static void AssembleExecutable(Config config, string nativeCode, string exeFilePath, string asmFilePath)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            File.WriteAllText(asmFilePath, nativeCode);
            var fasmIncludePath = Path.Combine(Path.GetDirectoryName(config.FasmPath), "INCLUDE");

            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.EnvironmentVariables["INCLUDE"] = fasmIncludePath;
            startInfo.FileName = config.FasmPath;
            startInfo.Arguments = asmFilePath + " " + exeFilePath;
            var fasmProcess = Process.Start(startInfo);
            fasmProcess.WaitForExit();

            stopWatch.Start();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>> FASM took " + stopWatch.ElapsedMilliseconds + " ms");
            Console.ResetColor();

            Assert.True(fasmProcess.ExitCode == 0, "FASM failed with code: " + fasmProcess.ExitCode);
        }

        private static string ReadInternalLib()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("erc.internal_lib.erc");
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

    }
}
