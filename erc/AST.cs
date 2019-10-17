using System;
using System.Collections.Generic;
using System.Globalization;

namespace erc
{
    public enum AstItemKind
    {
        Programm,
        VarDecl,
        Assignment,
        Immediate,
        Variable,
        VarScopeEnd,
        Vector,
        Expression,
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
        public string SourceLine { get; set; } //Source code line, only filled for statements
        public List<AstItem> Children { get => _children; set => _children = value; }
        public bool DataGenerated { get; set; } = false; //Used to track which immediates have already been generated in the data section

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

                case AstItemKind.VarScopeEnd:
                    return Kind + ": " + Identifier;

                case AstItemKind.Immediate:
                    return Kind + ": " + DataType + "(" + Value + ")";

                case AstItemKind.Vector:
                    var childValues = Children.ConvertAll((item) => item.ToString());
                    return "<" + String.Join(",", childValues) + ">";

                case AstItemKind.Variable:
                    return Kind + ": " + DataType + "(" + Identifier + ")";

                case AstItemKind.AddOp:
                case AstItemKind.SubOp:
                case AstItemKind.MulOp:
                case AstItemKind.DivOp:
                    return Kind + ": " + String.Join(", ", Children);
            }

            throw new Exception("Unknown expression kind: " + Kind);
        }

        public string ToSimpleString()
        {
            switch (Kind)
            {
                case AstItemKind.VarDecl:
                case AstItemKind.Assignment:
                case AstItemKind.Variable:
                    return Kind + ": \"" + Identifier + "\" (" + DataType + ")";

                case AstItemKind.VarScopeEnd:
                    return Kind + ": \"" + Identifier + "\"";

                case AstItemKind.Immediate:
                    return Kind + ": " + ImmediateValueToString() + " (" + DataType + ")";

                default:
                    return Kind.ToString();
            }

            throw new Exception("Unknown expression kind: " + Kind);
        }

        private string ImmediateValueToString()
        {
            if (Value == null)
                return "";

            if (Value is float fVal)
            {
                return fVal.ToString("0.0", CultureInfo.InvariantCulture);
            }
            else if (Value is double dVal)
            {
                return dVal.ToString("0.0", CultureInfo.InvariantCulture);
            }

            return Value.ToString();
        }

        public string ToTreeString()
        {
            return ToTreeStringRec(0);
        }

        private string ToTreeStringRec(int level)
        {
            var indent = new String(' ', level * 4);
            var result = indent + ToSimpleString() + Environment.NewLine;
            level += 1;
            foreach (var child in Children)
            {
                result += child.ToTreeStringRec(level);
            }
            return result;
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
            return Immediate(value, DataType.I64);
        }

        public static AstItem Immediate(float value)
        {
            return Immediate(value, DataType.F32);
        }

        public static AstItem Immediate(double value)
        {
            return Immediate(value, DataType.F64);
        }

        public static AstItem Variable(string varName, DataType dataType)
        {
            var result = new AstItem(AstItemKind.Variable);
            result.Identifier = varName;
            result.DataType = dataType;
            return result;
        }

        public static AstItem Vector(List<AstItem> values, DataType dataType)
        {
            var result = new AstItem(AstItemKind.Vector);
            result.Children.AddRange(values);
            result.DataType = dataType;
            return result;
        }
        
        public static AstItem Expression(DataType dataType, List<AstItem> children)
        {
            return new AstItem
            {
                Kind = AstItemKind.Expression,
                DataType = dataType,
                Children = children
            };
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

        public static AstItem VarScopeEnd(string varName)
        {
            return new AstItem { Kind = AstItemKind.VarScopeEnd, Identifier = varName };
        }

    }



}
