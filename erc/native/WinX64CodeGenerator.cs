using System;
using System.Collections.Generic;

namespace erc
{
    public class WinX64CodeGenerator
    {
        private CompilerContext _context;
        private X64FunctionFrame _functionScope;
        private X64MemoryManager _memoryManager = new X64MemoryManager();

        public void Generate(CompilerContext context)
        {
            _context = context;

            var asmSource = new List<string>();
            foreach (var obj in context.IMObjects)
            {
                switch (obj.Kind)
                {
                    case IMObjectKind.Function:
                        GenerateFunction(asmSource, obj as IMFunction);
                        break;

                    case IMObjectKind.ExternalFunction:
                        break;

                    default:
                        throw new Exception("");
                }
            }
        }

        private void GenerateFunction(List<string> output, IMFunction function)
        {
            output.Add("fn_" + function.Definition.Name + ":");

            _functionScope = _memoryManager.CreateFunctionScope(function);
            function.FunctionFrame = _functionScope;

            foreach (var operation in function.Body)
            {
                GenerateOperation(output, operation);
            }

            _functionScope = null;
        }



        private void GenerateOperation(List<string> output, IMOperation operation)
        {
            switch (operation.Instruction.Kind)
            {
                case IMInstructionKind.MOV:
                    GenerateMov(output, operation);
                    break;

                case IMInstructionKind.PUSH:
                    GeneratePush(output, operation);
                    break;

                case IMInstructionKind.POP:
                    GeneratePop(output, operation);
                    break;

                case IMInstructionKind.ADD:
                    GenerateAdd(output, operation);
                    break;

                case IMInstructionKind.SUB:
                    GenerateSub(output, operation);
                    break;

                case IMInstructionKind.MUL:
                    GenerateMul(output, operation);
                    break;

                case IMInstructionKind.DIV:
                    GenerateDiv(output, operation);
                    break;

            
                //default:
                    //throw new Exception("");
            }
        }

        private void GenerateMov(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GeneratePush(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GeneratePop(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateAdd(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateSub(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateMul(List<string> output, IMOperation operation)
        {
            //...
        }

        private void GenerateDiv(List<string> output, IMOperation operation)
        {
            //...
        }

        //Methods for all other operation kinds follow here
        //IDEA: The ones that need a lot of code could be in another file (partial class)

    }
}
