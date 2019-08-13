using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    public class Immediate
    {
        public DataType Type { get; set; }
        public object Value { get; set; }
    }

    public enum OperandType
    {
        Immediate, // => Immediate
        Variable // => String
    }

    public class Operand
    {
        public OperandType Type { get; set; }
        public object Value { get; set; }
    }

    public enum MathOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public class MathExpression
    {
        public MathOperator Operator { get; set; }
        public Operand Operand1 { get; set; }
        public Operand Operand2 { get; set; }
    }

    public enum ExpressionType
    {
        Immediate, // => Immediate
        Variable, // => String
        Math // => MathExpression
    }

    public class Expression
    {
        public ExpressionType Type { get; set; }
        public object Value { get; set; }
    }

    public enum StatementType
    {
        Definition, // => DefinitionStatement
        Assignment // => AssignmentStatement
    }

    public class DefinitionStatement
    {
        public String Variable { get; set; }
        public Expression Expression { get; set; }
    }

    public class AssignmentStatement
    {
        public String Variable { get; set; }
        public Expression Expression { get; set; }
    }

    public class Statement
    {
        public StatementType Type { get; set; }
        public object Value { get; set; }
    }

}
