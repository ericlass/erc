using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace erc
{
    public class Syntax
    {
        private CompilerContext _context;

        public Syntax()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;
            var tokens = new SimpleIterator<Token>(context.Tokens);
            var result = AstItem.Programm();

            var token = tokens.Current();
            while (token != null)
            {
                result.Children.Add(ReadStatement(tokens));
                token = tokens.Current();
            }

            context.AST = result;
        }

        private AstItem ReadStatement(SimpleIterator<Token> tokens)
        {
            var token = tokens.Current();

            if (token.TokenType != TokenType.Word)
            {
                throw new Exception("Expected identifier or 'let', found " + token.TokenType);
            }

            AstItem result = null;

            tokens.StartCapture();

            var first = token.Value;
            if (first == "let")
            {
                result = ReadVarDecl(tokens);
            }
            else
            {
                result = ReadAssignment(tokens);
            }

            var lineTokens = tokens.GetCapture();
            result.SourceLine = String.Join(" ", lineTokens.ConvertAll<string>((a) => a.Value));

            return result;
        }

        private AstItem ReadVarDecl(SimpleIterator<Token> tokens)
        {
            var let = tokens.Pop();

            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            if (_context.Variables.ContainsKey(name.Value))
                throw new Exception("Variable already defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens);
            var dataType = DataTypeOfExpression(expression);
            
            var variable = new Variable
            {
                DataType = dataType,
                Name = name.Value
            };

            _context.Variables.Add(name.Value, variable);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.VarDecl(variable.Name, variable.DataType, expression);
        }

        private AstItem ReadAssignment(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            if (!_context.Variables.ContainsKey(name.Value))
                throw new Exception("Variable not defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens);
            var variable = _context.Variables[name.Value];
            var expType = DataTypeOfExpression(expression);
            if (expType != variable.DataType)
                throw new Exception("Incompatible data types: " + variable.DataType + " <> " + expType);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.Assignment(variable.Name, variable.DataType, expression);
        }

        private DataType DataTypeOfExpression(AstItem expressionItem)
        {
            var dt = FindDataTypeOfExpression(expressionItem);
            if (dt == null)
                throw new Exception("Could not determine data type of expression: " + expressionItem);

            return dt;
        }

        private DataType FindDataTypeOfExpression(AstItem expressionItem)
        {
            switch (expressionItem.Kind)
            {
                case AstItemKind.Immediate:
                    if (expressionItem.Value is long)
                        return DataType.I64;
                    else if (expressionItem.Value is float)
                        return DataType.F32;
                    else if (expressionItem.Value is double)
                        return DataType.F64;
                    else
                        throw new Exception("Unknown immediate value: " + expressionItem.Value);

                case AstItemKind.Variable:
                    var varName = expressionItem.Identifier;
                    if (!_context.Variables.ContainsKey(varName))
                        throw new Exception("Variable not declared: " + varName);

                    return _context.Variables[varName].DataType;

                case AstItemKind.Vector:
                    var subType = FindDataTypeOfExpression(expressionItem.Children[0]);
                    if (subType.IsVector)
                        throw new Exception("Vectors of vectors are not allowed!");

                    var vectorSize = expressionItem.Children.Count;
                    var vectorType = DataType.GetVectorType(subType, vectorSize);

                    if (vectorType == DataType.VOID)
                        throw new Exception("Vectors of " + subType + " cannot have length " + vectorSize);

                    return vectorType;
            }

            foreach (var item in expressionItem.Children)
            {
                var current = FindDataTypeOfExpression(item);
                if (current != null)
                    return current;
            }

            return null;
        }

        private AstItem ReadExpression(SimpleIterator<Token> tokens)
        {
            var expTokens = new List<Token>();
            var tok = tokens.Current();
            while (tok != null && IsExpressionToken(tok))
            {
                expTokens.Add(tok);
                tokens.Step();
                tok = tokens.Current();
            }

            if (expTokens.Count == 0)
                throw new Exception("Expected expression, found " + tokens.Current());

            AstItem result = null;

            if (expTokens.Count == 1)
            {
                //Single immediate, vector or variable
                result = ReadSingleAstItem(expTokens[0]);
            }
            else
            {
                //Math Expression
                var expItemsInfix = new List<AstItem>();
                DataType expDataType = null;

                foreach (var token in expTokens)
                {
                    switch (token.TokenType)
                    {
                        case TokenType.Word:
                        case TokenType.Number:
                        case TokenType.Vector:
                            var operandItem = ReadSingleAstItem(token);
                            expItemsInfix.Add(operandItem);

                            if (expDataType == null)
                            {
                                expDataType = operandItem.DataType;
                            }
                            else
                            {
                                if (expDataType != operandItem.DataType)
                                    throw new Exception("All operands in an expressions must have the same data type! " + token + " has data type " + operandItem.DataType + " instead of " + expDataType);
                            }
                            break;

                        case TokenType.MathOperator:
                            var operatorItem = new AstItem(ParseOperator(token.Value));
                            expItemsInfix.Add(operatorItem);
                            break;

                        default:
                            throw new Exception("Unexpected expression token: " + token);
                    }
                }

                //Convert to postfix
                result = InfixToPostfix(expItemsInfix);
            }

            return result;
        }

        private AstItem ReadSingleAstItem(Token token)
        {
            AstItem result = null;

            if (token.TokenType == TokenType.Word)
            {
                Variable variable = null;
                if (_context.Variables.ContainsKey(token.Value))
                    variable = _context.Variables[token.Value];
                else
                    throw new Exception("Undefined variable: " + token);

                result = AstItem.Variable(token.Value, variable.DataType);
            }
            else if (token.TokenType == TokenType.Number)
            {
                var dataType = GuessDataType(token);
                var value = ParseNumber(token.Value, dataType);

                if (value is long)
                    result = AstItem.Immediate((long)value);
                else if (value is float)
                    result = AstItem.Immediate((float)value);
                else if (value is double)
                    result = AstItem.Immediate((double)value);
                else
                    throw new Exception("Unexpected number value type: " + value.GetType());
            }
            else if (token.TokenType == TokenType.Vector)
            {
                var values = new List<AstItem>();
                foreach (var vals in token.Values)
                {
                    var valExp = ReadExpression(new SimpleIterator<Token>(vals));
                    values.Add(valExp);
                }

                var subType = DataTypeOfExpression(values[0]);
                var vectorType = DataType.GetVectorType(subType, values.Count);
                if (vectorType == DataType.VOID)
                    throw new Exception("Not a valid vector type: " + subType + " x " + values.Count);

                result = AstItem.Vector(values, vectorType);

                //Check that all expressions have the same data type!
                if (values.Count > 0)
                {
                    var firstType = DataTypeOfExpression(values[0]);
                    for (int i = 1; i < values.Count; i++)
                    {
                        var currentType = DataTypeOfExpression(values[i]);
                        if (firstType != currentType)
                        {
                            throw new Exception("All expressions in vector must be of same type: " + result);
                        }
                    }
                }
            }

            return result;
        }

        private AstItemKind ParseOperator(string op)
        {
            switch (op)
            {
                case "+": return AstItemKind.AddOp;
                case "-": return AstItemKind.SubOp;
                case "*": return AstItemKind.MulOp;
                case "/": return AstItemKind.DivOp;

                default:
                    throw new Exception("Unsupported math operator: " + op);
            }
        }

        private bool IsExpressionToken(Token token)
        {
            return token.TokenType == TokenType.Word || token.TokenType == TokenType.Number || token.TokenType == TokenType.Vector || token.TokenType == TokenType.MathOperator;
        }

        private DataType GuessDataType(Token token)
        {
            if (token.TokenType == TokenType.Number)
            {
                var value = token.Value;
                if (!value.Contains('.'))
                    return DataType.I64;

                var last = value[value.Length - 1];

                if (last == 'f')
                    return DataType.F32;

                return DataType.F64;
            }

            throw new Exception("Cannot guess data type of: " + token);
        }

        private object ParseNumber(string str, DataType dataType)
        {
            var last = str[str.Length - 1];

            if (dataType == DataType.F32)
            {
                if (last == 'f')
                    return float.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return float.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.F64)
            {
                if (last == 'd')
                    return double.Parse(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
                else
                    return double.Parse(str, CultureInfo.InvariantCulture);
            }

            if (dataType == DataType.I64)
            {
                return long.Parse(str);
            }

            throw new Exception("Unsupported number type: " + dataType + " for value " + str);
        }

        /// <summary>
        /// Convert to the given expression in infix notation to postfix notation.
        /// </summary>
        /// <param name="infix">The expression in infix notation.</param>
        /// <returns>The expression converted to postfix notation.</returns>
        private AstItem InfixToPostfix(List<AstItem> infix)
        {
            var output = new List<AstItem>();
            var stack = new Stack<AstItem>();
            AstItem cbuffer = null;

            //Convert infix to postfix
            foreach (var item in infix)
            {
                if (item.Kind == AstItemKind.Immediate || item.Kind == AstItemKind.Variable || item.Kind == AstItemKind.Vector)
                {
                    output.Add(item);
                }
                /*else if (c == '(')
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
                }*/
                else
                {
                    if (stack.Count != 0 && Predecessor(stack.Peek(), item))
                    {
                        cbuffer = stack.Pop();
                        while (Predecessor(cbuffer, item))
                        {
                            output.Add(cbuffer);

                            if (stack.Count == 0)
                                break;

                            cbuffer = stack.Pop();
                        }
                        stack.Push(item);
                    }
                    else
                        stack.Push(item);
                }
            }

            while (stack.Count > 0)
            {
                cbuffer = stack.Pop();
                output.Add(cbuffer);
            }

            return AstItem.Expression(output[0].DataType, output);
        }

        /// <summary>
        /// Checks is firstOperator is a predecessor of secondOperator, meaning it has a higher or equal operator precedence.
        /// </summary>
        /// <param name="firstOperator">The first operator.</param>
        /// <param name="secondOperator">The second operator.</param>
        /// <returns></returns>
        private bool Predecessor(AstItem firstOperator, AstItem secondOperator)
        {
            return OperatorPrecedence(firstOperator) >= OperatorPrecedence(secondOperator);
        }

        /// <summary>
        /// Gets the precedence for the given operator.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <returns>The precedence.</returns>
        private int OperatorPrecedence(AstItem op)
        {
            switch (op.Kind)
            {
                case AstItemKind.AddOp:
                case AstItemKind.SubOp:
                    return 12;

                case AstItemKind.MulOp:
                case AstItemKind.DivOp:
                    return 13;

                default:
                    throw new Exception("Not an operator: " + op);
            }
        }

    }
}
