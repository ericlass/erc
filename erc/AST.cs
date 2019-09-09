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

    public enum AstItemKind
    {
        Programm,
        VarDecl,
        Assignment,
        Immediate,
        Variable,
        Array,
        AddOp,
        SubOp,
        MulOp,
        DivOp,
        //EqualsOp,
        //NotEqualsOp,
        //LessThanOp,
        //GreaterThanOp,
        //AndOp,
        //OrOp,
        //VarDecl,
        //Assignment,
        //FuncCall,
        //If
    }

    public class AstItem
    {
        private List<AstItem> _children = new List<AstItem>();

        public AstItemKind Kind { get; set; }
        public DataType DataType { get; set; }
        public string Identifier { get; set; } //Name of variable, function etc.
        public object Value { get; set; } //Value for immediates
        public List<AstItem> Children { get => _children; set => _children = value; }
        public AstItem()
        {
        }

        public AstItem(AstItemKind kind)
        {
            Kind = kind;
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case AstItemKind.Programm:
                    return Kind.ToString();

                case AstItemKind.VarDecl:
                case AstItemKind.Assignment:
                    return Kind + ": " + DataType + "(" + Identifier + "; " + Children[0] + ")";

                case AstItemKind.Immediate:
                    return Kind + ": " + DataType + "(" + Value + ")";

                case AstItemKind.Array:
                    var childValues = Children.ConvertAll((item) => item.ToString());
                    return "[" + String.Join(",", childValues) + "]";

                case AstItemKind.Variable:
                    return Kind + ": " + DataType + "(" + Identifier + ")";

                case AstItemKind.AddOp:
                case AstItemKind.SubOp:
                case AstItemKind.MulOp:
                case AstItemKind.DivOp:
                    return Kind.ToString();
            }

            throw new Exception("Unknown expression kind: " + Kind);
        }

        public static AstItem Programm()
        {
            return new AstItem(AstItemKind.Programm);
        }

        public static AstItem VarDecl(string varName, DataType dataType, AstItem expression)
        {
            var result = new AstItem(AstItemKind.VarDecl);
            result.Identifier = varName;
            result.DataType = dataType;
            result.Children.Add(expression);
            return result;
        }

        public static AstItem Assignment(string varName, DataType dataType, AstItem expression)
        {
            var result = new AstItem(AstItemKind.Assignment);
            result.Identifier = varName;
            result.DataType = dataType;
            result.Children.Add(expression);
            return result;
        }

        private static AstItem Immediate(object value, DataType dataType)
        {
            var result = new AstItem(AstItemKind.Immediate);
            result.DataType = dataType;
            result.Value = value;
            return result;
        }

        public static AstItem Immediate(long value)
        {
            return Immediate(value, DataType.i64);
        }

        public static AstItem Immediate(float value)
        {
            return Immediate(value, DataType.f32);
        }

        public static AstItem Immediate(double value)
        {
            return Immediate(value, DataType.f64);
        }

        public static AstItem Variable(string varName, DataType dataType)
        {
            var result = new AstItem(AstItemKind.Variable);
            result.Identifier = varName;
            result.DataType = dataType;
            return result;
        }

        public static AstItem Array(AstItem[] values)
        {
            var result = new AstItem(AstItemKind.Array);
            result.Children.AddRange(values);
            return result;
        }

        private static AstItem ArithmeticOp(AstItemKind op, AstItem op1, AstItem op2)
        {
            var result = new AstItem(op);
            result.Children.Add(op1);
            result.Children.Add(op2);
            return result;
        }

        public static AstItem AddOp(AstItem op1, AstItem op2)
        {
            return ArithmeticOp(AstItemKind.AddOp, op1, op2);
        }

        public static AstItem SubOp(AstItem op1, AstItem op2)
        {
            return ArithmeticOp(AstItemKind.SubOp, op1, op2);
        }

        public static AstItem MulOp(AstItem op1, AstItem op2)
        {
            return ArithmeticOp(AstItemKind.MulOp, op1, op2);
        }

        public static AstItem DivOp(AstItem op1, AstItem op2)
        {
            return ArithmeticOp(AstItemKind.DivOp, op1, op2);
        }

    }



}
