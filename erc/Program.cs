using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace erc
{
    class Program
    {
        static void Main(string[] args)
        {
            //var prefix = InfixToPrefix("a+b*c+d*e-f");
            var prefix = InfixToPrefix("a*((b+c))");

            Console.WriteLine(prefix);
            if (prefix != null)
                Console.Read();

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

        private static string InfixToPrefix(string inFix)
        {
            var output = new StringBuilder();
            var stack = new Stack<char>();
            var cbuffer = ' ';

            foreach (char c in inFix.ToCharArray())//Iterates characters in inFix
            {
                if (Char.IsLetterOrDigit(c))
                {
                    output.Append(c);
                }
                else if (c == '(')
                {
                    stack.Push(c);
                }
                else if (c == ')')
                {
                    cbuffer = stack.Pop();
                    while (cbuffer != '(')
                    {
                        output.Append(cbuffer);
                        cbuffer = stack.Pop();
                    }
                }
                else
                {
                    if (stack.Count != 0 && Predecessor(stack.Peek(), c))//If find an operator
                    {
                        cbuffer = stack.Pop();
                        while (Predecessor(cbuffer, c))
                        {
                            output.Append(cbuffer);

                            if (stack.Count == 0)
                                break;

                            cbuffer = stack.Pop();
                        }
                        stack.Push(c);
                    }
                    else
                        stack.Push(c);
                }
            }

            while (stack.Count > 0)
            {
                cbuffer = stack.Pop();
                output.Append(cbuffer);
            }

            var postFixStr = output.ToString().ToCharArray();
            Array.Reverse(postFixStr);
            return new string(postFixStr);
        }

        private static bool Predecessor(char firstOperator, char secondOperator)
        {
            string opString = "(+-*/%";

            int firstPoint, secondPoint;

            int[] precedence = { 0, 12, 12, 13, 13, 13 };

            firstPoint = opString.IndexOf(firstOperator);
            secondPoint = opString.IndexOf(secondOperator);

            return (precedence[firstPoint] >= precedence[secondPoint]) ? true : false;
        }

    }
}
