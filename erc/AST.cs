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
        DirectImmediate,
        Variable,
        VarScopeEnd,
        Vector,
        Expression,
        Operator,
        Parameter,
        ParameterList,
        StatementList,
        FunctionDecl,
        ExternFunctionDecl,
        FunctionCall,
        Return,
        If,
        For,
        NewPointer,
        DelPointer
    }

    public class AstItem
    {
        private List<AstItem> _children = new List<AstItem>();

        public AstItemKind Kind { get; set; }
        public DataType DataType { get; set; }
        public string Identifier { get; set; } //Name of variable, function etc.
        public object Value { get; set; } //Value for immediates
        public object Value2 { get; set; } //Value for immediates
        public IOperator Operator { get; set; } //Operator
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
                case AstItemKind.VarDecl:
                case AstItemKind.Assignment:
                    return Kind + ": " + DataType + "(" + Identifier + "; " + Children[0] + ")";

                case AstItemKind.VarScopeEnd:
                    return Kind + ": " + Identifier;

                case AstItemKind.Immediate:
                    return Kind + ": " + DataType + "(" + Value + ")";

                case AstItemKind.Vector:
                    return "<" + String.Join(",", Children) + ">";

                case AstItemKind.Variable:
                    return Kind + ": " + DataType + "(" + Identifier + ")";

                case AstItemKind.Operator:
                    return Kind + ": " + Operator.Figure;

                case AstItemKind.Parameter:
                    return Kind + ": " + Identifier + "(" + DataType + ")";

                case AstItemKind.ParameterList:
                case AstItemKind.StatementList:
                    return Kind + ": " + String.Join(", ", Children);

                case AstItemKind.FunctionDecl:
                case AstItemKind.FunctionCall:
                    return Kind + ": " + Identifier + "(" + String.Join(", ", Children) + ")";

                default:
                    return Kind.ToString();
            }
        }

        public string ToSimpleString()
        {
            switch (Kind)
            {
                case AstItemKind.VarDecl:
                case AstItemKind.Assignment:
                case AstItemKind.Variable:
                case AstItemKind.Parameter:
                    return Kind + ": \"" + Identifier + "\" (" + DataType + ")";

                case AstItemKind.VarScopeEnd:
                case AstItemKind.FunctionCall:
                case AstItemKind.FunctionDecl:
                    return Kind + ": \"" + Identifier + "\"";

                case AstItemKind.Immediate:
                    return Kind + ": " + ImmediateValueToString() + " (" + DataType + ")";

                case AstItemKind.Operator:
                    return Kind + ": " + this.Operator.Figure;

                default:
                    return Kind.ToString();
            }
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
                if (child != null)
                    result += child.ToTreeStringRec(level);
            }
            return result;
        }

        public static AstItem Programm()
        {
            return new AstItem(AstItemKind.Programm);
        }

        public static AstItem VarDecl(string varName, AstItem expression)
        {
            var result = new AstItem(AstItemKind.VarDecl);
            result.Identifier = varName;
            result.Children.Add(expression);
            return result;
        }

        public static AstItem Assignment(string varName, AstItem expression)
        {
            var result = new AstItem(AstItemKind.Assignment);
            result.Identifier = varName;
            result.Children.Add(expression);
            return result;
        }

        public static AstItem Immediate(object value)
        {
            var result = new AstItem(AstItemKind.Immediate);
            result.Value = value;
            return result;
        }

        public static AstItem DirectImmediate(long value)
        {
            var result = new AstItem(AstItemKind.DirectImmediate);
            result.Value = value;
            result.DataType = DataType.U64;
            return result;
        }

        public static AstItem Variable(string varName)
        {
            var result = new AstItem(AstItemKind.Variable);
            result.Identifier = varName;
            return result;
        }

        public static AstItem Vector(string name, List<AstItem> values)
        {
            var result = new AstItem(AstItemKind.Vector);
            result.Children.AddRange(values);
            result.Identifier = name;
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

        public static AstItem VarScopeEnd(string varName)
        {
            return new AstItem { Kind = AstItemKind.VarScopeEnd, Identifier = varName };
        }

        public static AstItem Parameter(string name, DataType dataType)
        {
            return new AstItem { Kind = AstItemKind.Parameter, Identifier = name, DataType = dataType };
        }

        public static AstItem ParameterList(List<AstItem> parameters)
        {
            if (parameters != null && !parameters.TrueForAll((p) => p.Kind == AstItemKind.Parameter))
                throw new Exception("All parameters must be Kind = Parameter!");

            return new AstItem { Kind = AstItemKind.ParameterList, Children = parameters };
        }

        public static AstItem StatementList(List<AstItem> statements)
        {
            //TODO: Check that all items in given list are statements
            return new AstItem { Kind = AstItemKind.StatementList, Children = statements };
        }

        public static AstItem FunctionDecl(string name, DataType returnType, List<AstItem> parameters, List<AstItem> statements)
        {
            var paramList = ParameterList(parameters);
            var statementList = StatementList(statements);

            return new AstItem { Kind = AstItemKind.FunctionDecl, Identifier = name, DataType = returnType, Children = new List<AstItem>() { paramList, statementList } };
        }

        public static AstItem ExternFunctionDecl(string name, DataType returnType, List<AstItem> parameters, string libFnName, string libName)
        {
            var paramList = ParameterList(parameters);
            return new AstItem { Kind = AstItemKind.ExternFunctionDecl, Identifier = name, DataType = returnType, Value = libName, Value2 = libFnName, Children = new List<AstItem>() { paramList } };
        }

        public static AstItem FunctionCall(string name, List<AstItem> parameterValues)
        {
            return new AstItem { Kind = AstItemKind.FunctionCall, Identifier = name, Children = parameterValues };
        }

        public static AstItem Return(DataType dataType, AstItem value)
        {
            return new AstItem { Kind = AstItemKind.Return, DataType = dataType, Children = new List<AstItem> { value } };
        }

        internal static AstItem ForLoop(string varName, AstItem startExpression, AstItem endExpression, List<AstItem> statements)
        {
            var statementList = StatementList(statements);
            return new AstItem { Kind = AstItemKind.FunctionDecl, Identifier = varName, Children = new List<AstItem>() { startExpression, endExpression, statementList } };
        }

        public static AstItem AsOperator(IOperator oper)
        {
            return new AstItem { Kind = AstItemKind.Operator, Operator = oper };
        }

        public static AstItem IfStatement(AstItem expression, List<AstItem> statements, List<AstItem> elseStatements)
        {
            var statementList = StatementList(statements);

            AstItem elseStatementList = null;
            if (elseStatements != null)
                elseStatementList = StatementList(elseStatements);

            return new AstItem { Kind = AstItemKind.If, Children = new List<AstItem>() { expression, statementList, elseStatementList } };
        }

        public static AstItem NewPointer(DataType dataType, object amount)
        {
            return new AstItem { Kind = AstItemKind.NewPointer, DataType = dataType, Value = amount };
        }

        public static AstItem DelPointer(string varName)
        {
            return new AstItem { Kind = AstItemKind.DelPointer, DataType = DataType.VOID, Identifier = varName };
        }

    }
}
