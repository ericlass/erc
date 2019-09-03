using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erc
{
    /*
    public class Immediate
    {
        public DataType Type { get; set; }
        public object Value { get; set; }

        public List<Expression> ValueAsArray()
        {
            if (Type != DataType.Array)
                throw new Exception("Immediate is not an array but a " + Type);

            return (List<Expression>)Value;
        }

        public override string ToString()
        {
            var valueStr = Value.ToString();
            if (Type == DataType.Array)
            {
                valueStr = "[" + String.Join(", ", Value as List<Expression>) + "]";
            }
            return Type + "(\"" + valueStr + "\")";
        }

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

        public override string ToString()
        {
            return Type + "(" + Value + ")";
        }
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

        public override string ToString()
        {
            return "'" + Operand1 + "' '" + Operator + "' '" + Operand2 + "'";
        }
    }

    public enum ExpressionType
    {
        Immediate, // => Immediate
        Variable, // => Variable
        Math // => MathExpression
    }
    */

    public enum ExpItemKind
    {
        Immediate,
        Variable,
        AddOp,
        SubOp,
        MulOp,
        DivOp,
        FuncCall
    }

    public class ExpressionItem
    {
        public ExpItemKind Kind { get; set; }
        public DataType DataType { get; set; }
        public object Value { get; set; } //For immediates (int, long, string... but not for arrays!)
        public string Identifier { get; set; } //Name of variable, function etc.
        public List<ExpressionItem> Children { get; set; } //Values for arrays, math expression, parameters for function calls etc.

        public override string ToString()
        {
            switch (Kind)
            {
                case ExpItemKind.Immediate:
                    return Kind + ": " + DataType + "(" + Value + ")";
                case ExpItemKind.Variable:
                    return Kind + ": " + DataType + "(" + Identifier + ")";
                case ExpItemKind.AddOp:
                case ExpItemKind.SubOp:
                case ExpItemKind.MulOp:
                case ExpItemKind.DivOp:
                    return Kind.ToString();
                case ExpItemKind.FuncCall:
                    return Kind + ": " + "(" + Identifier + ") [" + String.Join(";", Children) + "]";
            }

            throw new Exception("Unknown expression kind: " + Kind);
        }
    }

    public enum StatementKind
    {
        VarDecl, // => VarDeclStatement
        Assignment // => AssignmentStatement
    }

    public class Statement
    {
        public StatementKind Kind { get; set; }
        public VarDeclStatement VarDecl { get; set; }
        public AssignmentStatement Assignment { get; set; }

        public override string ToString()
        {
            string value = null;
            switch (Kind)
            {
                case StatementKind.VarDecl:
                    value = VarDecl.ToString();
                    break;
                case StatementKind.Assignment:
                    value = Assignment.ToString();
                    break;
                default:
                    throw new Exception("Unknown statement kind: " + Kind);
            }

            return Kind + ": " + value;
        }
    }

    public class VarDeclStatement : AssignmentStatement
    {
    }

    public class AssignmentStatement
    {
        public Variable Variable { get; set; }
        public ExpressionItem Expression { get; set; }

        public override string ToString()
        {
            return Variable + " = " + Expression;
        }
    }

}
