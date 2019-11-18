using System;
using System.Collections.Generic;
using System.Globalization;

namespace erc
{
    public class Syntax
    {
        private CompilerContext _context;
        private Function _currentFunction;

        public Syntax()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;
            var tokens = new SimpleIterator<Token>(context.Tokens);
            var result = AstItem.Programm();

            result.Children = Read(tokens);

            context.AST = result;
        }

        private List<AstItem> Read(SimpleIterator<Token> tokens)
        {
            var result = new List<AstItem>();
            var token = tokens.Current();
            while (token != null)
            {
                switch (token.TokenType)
                {
                    case TokenType.Fn:
                        result.Add(ReadFuncDecl(tokens));
                        break;

                    default:
                        throw new Exception("Unexpected token. Expected Fn, found: " + token);
                }

                token = tokens.Current();
            }
            return result;
        }

        private AstItem ReadFuncDecl(SimpleIterator<Token> tokens)
        {
            //Skip "fn"
            tokens.Pop();

            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            var openBracket = tokens.Pop();
            List<AstItem> parameters = new List<AstItem>();
            if (openBracket.TokenType == TokenType.RoundBracketOpen)
                parameters = ReadFuncParameters(tokens);
            else
                throw new Exception("Expected '(', found " + openBracket);

            var next = tokens.Pop();
            DataType returnType = DataType.VOID;
            List<AstItem> statements = new List<AstItem>();

            if (next.TokenType == TokenType.TypeOperator)
            {
                returnType = ReadDataType(tokens);
                next = tokens.Pop();
            }

            if (next.TokenType == TokenType.CurlyBracketOpen)
                statements = ReadStatements(tokens);
            else
                throw new Exception("Expected ':' or '{', found " + next);

            next = tokens.Pop();
            if (next.TokenType != TokenType.CurlyBracketClose)
                throw new Exception("Expected '}', found " + next);

            if (_context.GetFunction(name.Value) != null)
                throw new Exception("Function with name '" + name.Value + "' already defined!");

            var funcParams = parameters.ConvertAll((p) => new FunctionParameter(p.Identifier, p.DataType));
            var function = new Function(name.Value, returnType, funcParams);
            _context.AddFunction(function);

            return AstItem.FunctionDecl(name.Value, returnType, parameters, statements);
        }

        private List<AstItem> ReadFuncParameters(SimpleIterator<Token> tokens)
        {
            var result = new List<AstItem>();
            var current = tokens.Pop();
            while (current.TokenType != TokenType.RoundBracketClose)
            {
                var name = current;
                if (name.TokenType != TokenType.Word)
                    throw new Exception("Expected parameter name, found " + name);

                var colon = tokens.Pop();
                if (colon.TokenType != TokenType.TypeOperator)
                    throw new Exception("Expected ':', found " + colon);

                var dataType = ReadDataType(tokens);

                result.Add(AstItem.Parameter(name.Value, dataType));

                current = tokens.Pop();
                if (current.TokenType == TokenType.Comma)
                    current = tokens.Pop();
            }
            return result;
        }

        private List<AstItem> ReadStatements(SimpleIterator<Token> tokens)
        {
            var result = new List<AstItem>();
            var token = tokens.Current();
            while (token != null)
            {
                var statement = ReadStatement(tokens);
                if (statement == null)
                    break;

                result.Add(statement);
                token = tokens.Current();
            }
            return result;
        }

        private AstItem ReadStatement(SimpleIterator<Token> tokens)
        {
            var token = tokens.Current();

            AstItem result = null;

            tokens.StartCapture();

            switch (token.TokenType)
            {
                case TokenType.Let:
                    result = ReadVarDecl(tokens);
                    break;

                case TokenType.Ret:
                    tokens.Pop();
                    result = ReadExpression(tokens, TokenType.StatementTerminator);
                    break;

                case TokenType.Word:
                    var next = tokens.Next();
                    if (next.TokenType == TokenType.RoundBracketOpen)
                        result = ReadFuncCall(tokens);
                    else if (next.TokenType == TokenType.AssigmnentOperator)
                        result = ReadAssignment(tokens);
                    else
                        throw new Exception("Unexpected token after identifier '" + token.Value + "'. Expected '=' or '(', found: " + next);
                    break;

                case TokenType.CurlyBracketClose:
                    //End of function
                    return null;

                default:
                    throw new Exception("Unexpected token. Expected 'let' or identifier, found: " + token);
            }

            var lineTokens = tokens.GetCapture();
            if (lineTokens.Count > 0)
                result.SourceLine = String.Join(" ", lineTokens.ConvertAll<string>((a) => a.Value));

            return result;
        }

        private AstItem ReadFuncCall(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();

            var function = _context.GetFunction(name.Value);
            if (function == null)
                throw new Exception("Undeclared function: " + name.Value);

            var bracket = tokens.Pop();
            if (bracket.TokenType != TokenType.RoundBracketOpen)
                throw new Exception("Unexcepted token after function name! Expected '(', found: " + bracket);

            var paramValues = ReadTokenList(tokens, TokenType.Comma, TokenType.RoundBracketClose);
            if (paramValues.Count != function.Parameters.Count)
                throw new Exception("Invalid number of arguments to function '" + function.Name + "'! Expected: " + function.Parameters.Count + ", given: " + paramValues.Count);

            var paramExpressions = new List<AstItem>(paramValues.Count);
            foreach (var valueTokens in paramValues)
            {
                var expression = ReadExpression(new SimpleIterator<Token>(valueTokens), null);
                paramExpressions.Add(expression);
            }

            for (int i = 0; i < paramExpressions.Count; i++)
            {
                var expression = paramExpressions[i];
                var parameter = function.Parameters[i];

                if (expression.DataType != parameter.DataType)
                    throw new Exception("Invalid type for parameter '" + parameter.Name + "'! Expected: " + parameter.DataType + ", given: " + expression.DataType);
            }

            return AstItem.FunctionCall(function.Name, function.ReturnType, paramExpressions);
        }

        private List<List<Token>> ReadTokenList(SimpleIterator<Token> tokens, TokenType separator, TokenType terminator)
        {
            var result = new List<List<Token>>();
            var expTokens = new List<Token>();

            var token = tokens.Current();
            if (token.TokenType == terminator)
                return result;

            while (token != null)
            {
                if (token.TokenType == separator)
                {
                    result.Add(expTokens);
                    expTokens = new List<Token>();
                }
                else if (token.TokenType == terminator)
                {
                    result.Add(expTokens);
                    break;
                }
                else
                    expTokens.Add(token);

                token = tokens.Pop();
            }

            return result;
        }

        private AstItem ReadVarDecl(SimpleIterator<Token> tokens)
        {
            var let = tokens.Pop();

            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            if (_context.GetVariable(name.Value) != null)
                throw new Exception("Variable already defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens, TokenType.StatementTerminator);
            var dataType = DataTypeOfExpression(expression);

            var variable = new Variable
            {
                DataType = dataType,
                Name = name.Value
            };

            _context.AddVariable(variable);

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

            var variable = _context.GetVariable(name.Value);
            if (variable == null)
                throw new Exception("Variable not defined: " + name.Value);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator || op.Value != "=")
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens, TokenType.StatementTerminator);
            var expType = DataTypeOfExpression(expression);
            if (expType != variable.DataType)
                throw new Exception("Incompatible data types: " + variable.DataType + " <> " + expType);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.Assignment(variable.Name, variable.DataType, expression);
        }

        private DataType ReadDataType(SimpleIterator<Token> tokens)
        {
            var typeName = tokens.Pop();
            if (typeName.TokenType != TokenType.Word)
                throw new Exception("Expected data type, found " + typeName);

            var name = typeName.Value;
            var result = DataType.GetAllValues().Find((t) => t.Name == name);

            if (result == null)
                throw new Exception("Unkown type: " + name);

            return result;
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
                    var variable = _context.GetVariable(varName);
                    if (variable == null)
                        throw new Exception("Variable not declared: " + varName);

                    return variable.DataType;

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

        private AstItem ReadExpression(SimpleIterator<Token> tokens, Nullable<TokenType> terminator)
        {
            var expTokens = new List<Token>();
            if (terminator == null)
            {
                expTokens.AddRange(tokens.ToList());
            }
            else
            {
                var tok = tokens.Current();
                while (tok != null && tok.TokenType != terminator.Value)
                {
                    expTokens.Add(tok);
                    tokens.Step();
                    tok = tokens.Current();
                }
            }

            if (expTokens.Count == 0)
                throw new Exception("Expected expression, found " + tokens.Current());

            AstItem result = null;
            var tokenIter = new SimpleIterator<Token>(expTokens);

            if (expTokens.Count == 1)
            {
                //Single immediate, vector or variable
                result = ReadSingleAstItem(tokenIter);
            }
            else
            {
                //Math Expression
                var expItemsInfix = new List<AstItem>();
                DataType expDataType = null;

                var token = tokenIter.Current();
                while (token != null)
                {
                    switch (token.TokenType)
                    {
                        case TokenType.Word:
                        case TokenType.Number:
                        case TokenType.Vector:
                            var operandItem = ReadSingleAstItem(tokenIter);
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

                        case TokenType.RoundBracketOpen:
                            expItemsInfix.Add(AstItem.RoundBracketOpen());
                            break;

                        case TokenType.RoundBracketClose:
                            expItemsInfix.Add(AstItem.RoundBracketClose());
                            break;

                        default:
                            throw new Exception("Unexpected expression token: " + token);
                    }

                    tokenIter.Step();
                    token = tokenIter.Current();
                }

                expItemsInfix.ForEach((a) => a.DataType = expDataType);

                //Convert to postfix
                result = InfixToPostfix(expItemsInfix);
            }

            return result;
        }

        private AstItem ReadSingleAstItem(SimpleIterator<Token> tokens)
        {
            AstItem result = null;
            var token = tokens.Current();

            if (token.TokenType == TokenType.Word)
            {
                var next = tokens.Next();
                if (next != null && next.TokenType == TokenType.RoundBracketOpen)
                {
                    result = ReadFuncCall(tokens);
                }
                else
                {
                    Variable variable = _context.GetVariable(token.Value);
                    if (variable == null)
                        throw new Exception("Undefined variable: " + token);

                    result = AstItem.Variable(token.Value, variable.DataType);
                }
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
                    var valExp = ReadExpression(new SimpleIterator<Token>(vals), null);
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

        private DataType GuessDataType(Token token)
        {
            if (token.TokenType == TokenType.Number)
            {
                var value = token.Value;
                if (!value.Contains("."))
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
                else if (item.Kind == AstItemKind.RoundBracketOpen)
                {
                    stack.Push(item);
                }
                else if (item.Kind == AstItemKind.RoundBracketClose)
                {
                    cbuffer = stack.Pop();
                    while (cbuffer.Kind != AstItemKind.RoundBracketOpen)
                    {
                        output.Add(cbuffer);
                        cbuffer = stack.Pop();
                    }
                }
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
                case AstItemKind.RoundBracketOpen:
                case AstItemKind.RoundBracketClose:
                    return 11;

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
