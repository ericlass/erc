using System;
using System.Collections.Generic;


namespace erc
{
    public class Optimizer
    {
        List<Operation> _operations = null;

        public void Optimize(List<Operation> operations)
        {
            _operations = operations;

            ReduceMoves();
        }

        private void ReduceMoves()
        {
            for (int i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i];
                var instruction = operation.Instruction;

                //TODO: Make this work for all cases, all instructions
                if (instruction == Instruction.VADDPD)
                {
                    var target = operation.Operand1;
                    var operand1 = operation.Operand2;
                    var operand2 = operation.Operand3;

                    if (operand1.Kind == StorageLocationKind.Register)
                    {
                        for (int j = 1; j <= 2; j++)
                        {
                            var operation2 = _operations[i - j];
                            if (operation2.Instruction == Instruction.VMOVAPD && operation2.Operand1 == operand1 && operation2.Operand2.Kind == StorageLocationKind.Register && operation2.Operand2.Register.ByteSize == operand1.Register.ByteSize)
                            {
                                operation.Operand2 = operation2.Operand2;
                                _operations.RemoveAt(i - j);
                                i = i - 1;
                                break;
                            }
                        }
                    }

                    if (operand2.Kind == StorageLocationKind.Register)
                    {
                        for (int j = 1; j <= 2; j++)
                        {
                            var operation2 = _operations[i - j];
                            if (operation2.Instruction == Instruction.VMOVAPD && operation2.Operand1 == operand2 && operation2.Operand2.Kind == StorageLocationKind.Register && operation2.Operand2.Register.ByteSize == operand2.Register.ByteSize)
                            {
                                operation.Operand3 = operation2.Operand2;
                                _operations.RemoveAt(i - j);
                                i = i - 1;
                                break;
                            }
                        }
                    }

                    if (target.Kind == StorageLocationKind.Register)
                    {
                        for (int j = 1; j <= 1 && i + j < _operations.Count; j++)
                        {
                            var operation2 = _operations[i + j];
                            if (operation2.Instruction == Instruction.VMOVAPD && operation2.Operand2 == target && operation2.Operand1.Kind == StorageLocationKind.Register && operation2.Operand1.Register.ByteSize == target.Register.ByteSize)
                            {
                                operation.Operand1 = operation2.Operand1;
                                _operations.RemoveAt(i + j);
                                //i = i - 1;
                                break;
                            }
                        }
                    }

                }
            }
        }

    }
}
