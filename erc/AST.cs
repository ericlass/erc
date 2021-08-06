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
        Vector,
        Expression,
        BinaryOperator,
        Parameter,
        ParameterList,
        StatementList,
        FunctionDecl,
        ExternFunctionDecl,
        FunctionCall,
        Return,
        If,
        For,
        While,
        Break,
        NewRawPointer,
        DelPointer,
        IndexAccess,
        UnaryOperator,
        // Used as target for assignments.
        PointerDeref,
        EnumDecl,
        EnumElement,
        Identifier,
        Type,
        ValueArrayDefinition,
        SizedArrayDefinition,
        NewStackArray,
        NewHeapArray,
    }

    public class AstItem
    {
        public AstItemKind Kind { get; set; }
        public DataType DataType { get; set; }
        public string Identifier { get; set; } //Name of variable, function etc.
        public object Value { get; set; } //Value for immediates
        public object Value2 { get; set; } //Value for immediates
        public IOperator Operator { get; set; } //Operator
        public string SourceLine { get; set; } //Source code line, only filled for statements
        public List<AstItem> Children { get; set; } = new List<AstItem>();

        public IBinaryOperator BinaryOperator => (IBinaryOperator)Operator;
        public IUnaryOperator UnaryOperator => (IUnaryOperator)Operator;

        public AstItem()
        {
        }

        public AstItem(AstItemKind kind)
        {
            Kind = kind;
        }

        public AstItem Copy()
        {
            return new AstItem
            {
                Kind = Kind,
                DataType = DataType,
                Identifier = Identifier,
                Value = Value,
                Value2 = Value2,
                Operator = Operator,
                SourceLine = SourceLine,
                Children = new List<AstItem>(Children)
            };
        }

        public override string ToString()
        {
            return Kind switch
            {
                AstItemKind.VarDecl or AstItemKind.Assignment
                    => Kind + ": " + DataType + "(" + Children[0] + ")",
                AstItemKind.Immediate
                    => Kind + ": " + DataType + "(" + Value + ")",
                AstItemKind.SizedArrayDefinition
                    => Kind + ": " + DataType.ElementType + " * " + Children[1],
                AstItemKind.ValueArrayDefinition
                    => Kind + ": [" + String.Join(",", Children) + "]",
                AstItemKind.Vector
                    => "<" + String.Join(",", Children) + ">",
                AstItemKind.Variable or AstItemKind.PointerDeref or AstItemKind.IndexAccess 
                    => Kind + ": " + DataType + "(" + Identifier + ")",
                AstItemKind.BinaryOperator or AstItemKind.UnaryOperator
                    => Kind + ": " + Operator.Figure,
                AstItemKind.Parameter
                    => Kind + ": " + Identifier + "(" + DataType + ")",
                AstItemKind.ParameterList or AstItemKind.StatementList
                    => Kind + ": " + String.Join(", ", Children),
                AstItemKind.FunctionDecl or AstItemKind.FunctionCall or AstItemKind.ExternFunctionDecl
                    => Kind + ": " + Identifier + "(" + String.Join(", ", Children) + ")",
                AstItemKind.Type
                    => Kind + ": " + DataType.ElementType.Name,
                AstItemKind.Identifier
                    => Kind + ": " + Identifier,
                _ => Kind.ToString(),
            };
        }

        public string ToSimpleString()
        {
            return Kind switch
            {
                AstItemKind.VarDecl or AstItemKind.Variable or AstItemKind.Parameter or AstItemKind.PointerDeref or AstItemKind.IndexAccess
                    => Kind + ": \"" + Identifier + "\" (" + DataType + ")",
                AstItemKind.FunctionCall or AstItemKind.EnumDecl
                    => Kind + ": \"" + Identifier + "\"",
                AstItemKind.EnumElement
                    => Kind + ": \"" + Identifier + "\" (" + Value + ")",
                AstItemKind.FunctionDecl
                    => Kind + ": \"" + Identifier + "\" (" + DataType + ")",
                AstItemKind.ExternFunctionDecl
                    => Kind + ": \"" + Identifier + "\" (" + DataType + ") [\"" + Value2 + "\"; \"" + Value + "\"]",
                AstItemKind.Immediate
                    => Kind + ": " + ImmediateValueToString() + " (" + DataType + ")",
                AstItemKind.BinaryOperator or AstItemKind.UnaryOperator
                    => Kind + ": " + this.Operator.Figure,
                AstItemKind.Type or AstItemKind.SizedArrayDefinition or AstItemKind.ValueArrayDefinition or AstItemKind.NewRawPointer   
                    => Kind + ": (" + DataType.Name + ")",
                AstItemKind.Identifier
                    => Kind + ": " + Identifier,
                _ => Kind.ToString(),
            };
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
            else if (Value is string strVal)
            {
                if (strVal.Length == 1)
                    return "'" + StringUtils.CharToPrintableStr(strVal[0]) + "'";
                else
                    return "\"" + StringUtils.Escape(strVal) + "\"";
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
            var result = new AstItem(AstItemKind.VarDecl)
            {
                Identifier = varName
            };
            result.Children.Add(expression);
            return result;
        }

        public static AstItem VariableAssignment(string varName, AstItem expression)
        {
            var result = new AstItem(AstItemKind.Assignment);
            result.Children.Add(Variable(varName));
            result.Children.Add(expression);
            return result;
        }

        public static AstItem PointerAssignment(string varName, AstItem expression)
        {
            var pointer = PointerDeref(varName);

            var result = new AstItem(AstItemKind.Assignment);
            result.Children.Add(pointer);
            result.Children.Add(expression);

            return result;
        }

        public static AstItem PointerIndexAssignment(string varName, AstItem indexExpression, AstItem expression)
        {
            var pointer = IndexAccess(varName, indexExpression);

            var result = new AstItem(AstItemKind.Assignment);
            result.Children.Add(pointer);
            result.Children.Add(expression);

            return result;
        }

        public static AstItem Immediate(object value)
        {
            var result = new AstItem(AstItemKind.Immediate)
            {
                Value = value
            };
            return result;
        }

        public static AstItem Immediate(DataType dataType, object value)
        {
            var result = new AstItem(AstItemKind.Immediate)
            {
                Value = value,
                DataType = dataType
            };
            return result;
        }

        public static AstItem Variable(string varName)
        {
            var result = new AstItem(AstItemKind.Variable)
            {
                Identifier = varName
            };
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

        public static AstItem ForLoop(string varName, AstItem startExpression, AstItem endExpression, AstItem incExpression, List<AstItem> statements)
        {
            var statementList = StatementList(statements);
            return new AstItem { Kind = AstItemKind.For, Identifier = varName, Children = new List<AstItem>() { startExpression, endExpression, incExpression, statementList } };
        }

        public static AstItem WhileLoop(AstItem whileExpression, List<AstItem> statements)
        {
            var statementList = StatementList(statements);
            return new AstItem { Kind = AstItemKind.While, Children = new List<AstItem>() { whileExpression, statementList } };
        }

        public static AstItem Break()
        {
            return new AstItem { Kind = AstItemKind.Break };
        }

        public static AstItem AsOperator(IBinaryOperator oper)
        {
            return new AstItem { Kind = AstItemKind.BinaryOperator, Operator = oper };
        }
        public static AstItem AsUnaryOperator(IUnaryOperator oper)
        {
            return new AstItem { Kind = AstItemKind.UnaryOperator, Operator = oper };
        }

        public static AstItem IfStatement(AstItem expression, List<AstItem> statements, List<AstItem> elseStatements)
        {
            var statementList = StatementList(statements);

            AstItem elseStatementList = null;
            if (elseStatements != null)
                elseStatementList = StatementList(elseStatements);

            return new AstItem { Kind = AstItemKind.If, Children = new List<AstItem>() { expression, statementList, elseStatementList } };
        }

        public static AstItem NewRawPointer(DataType dataType, AstItem amountExpression)
        {
            return new AstItem { Kind = AstItemKind.NewRawPointer, DataType = dataType, Children = new List<AstItem>() { amountExpression } };
        }

        public static AstItem DelPointer(string varName)
        {
            return new AstItem { Kind = AstItemKind.DelPointer, DataType = DataType.VOID, Identifier = varName };
        }

        private static AstItem PointerDeref(string varName)
        {
            return new AstItem { Kind = AstItemKind.PointerDeref, Identifier = varName };
        }

        public static AstItem IndexAccess(string varName, AstItem indexExpression)
        {
            //Data type is set by semantic analysis
            return new AstItem { Kind = AstItemKind.IndexAccess, Identifier = varName, Children = { indexExpression } };
        }

        public static AstItem EnumElement(string elementName, int index)
        {
            return new AstItem { Kind = AstItemKind.EnumElement, Identifier = elementName, Value = index };
        }

        public static AstItem EnumDecl(string enumName, List<AstItem> enumElements)
        {
            if (!enumElements.TrueForAll((e) => e.Kind == AstItemKind.EnumElement))
                throw new Exception("All enum element AST items must be of kind EnumElement!");

            return new AstItem { Kind = AstItemKind.EnumDecl, Identifier = enumName, Children = enumElements };
        }

        public static AstItem AsIdentifier(string identifier)
        {
            return new AstItem { Kind = AstItemKind.Identifier, Identifier = identifier };
        }

        public static AstItem AsType(DataType dataType)
        {
            return new AstItem { Kind = AstItemKind.Type, DataType = dataType };
        }

        public static AstItem ValueArrayDefinition(List<AstItem> valueExpressions)
        {
            return new AstItem { Kind = AstItemKind.ValueArrayDefinition, Children = valueExpressions };
        }

        public static AstItem SizedArrayDefinition(AstItem initialValue, AstItem numItemsExpression)
        {
            return new AstItem { Kind = AstItemKind.SizedArrayDefinition, Children = new List<AstItem>() { initialValue, numItemsExpression } };
        }

        public static AstItem NewStackArray(AstItem arrayDefinition)
        {
            return new AstItem { Kind = AstItemKind.NewStackArray, Children = new List<AstItem>() { arrayDefinition } };
        }

        public static AstItem NewHeapArray(AstItem arrayDefinition)
        {
            return new AstItem { Kind = AstItemKind.NewHeapArray, Children = new List<AstItem>() { arrayDefinition } };
        }

    }
}

