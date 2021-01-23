using System;
using System.Collections.Generic;

namespace erc
{
    public class SyntaxAnalysis
    {
        public SyntaxAnalysis()
        {
        }

        public void Analyze(CompilerContext context)
        {
            var tokens = new TokenIterator(context.Tokens);
            var result = AstItem.Programm();
            result.Children = Read(tokens);
            context.AST = result;
        }

        private List<AstItem> Read(TokenIterator tokens)
        {
            var result = new List<AstItem>();
            var token = tokens.Current();
            while (token != null)
            {
                switch (token.Kind)
                {
                    case TokenKind.Fn:
                        result.Add(ReadFuncDecl(tokens));
                        break;

                    case TokenKind.Enum:
                        result.Add(ReadEnumDecl(tokens));
                        break;

                    default:
                        throw new Exception("Unexpected token. Expected Fn, found: " + token);
                }

                token = tokens.Current();
            }
            return result;
        }

        private AstItem ReadEnumDecl(TokenIterator tokens)
        {
            //Skip "enum"
            tokens.Pop();

            var enumName = tokens.PopExpected(TokenKind.Word).Value;
            
            tokens.PopExpected(TokenKind.CurlyBracketOpen);

            var currentIndex = 0;
            var elements = new List<AstItem>();
            var current = tokens.Current();
            while (current.Kind != TokenKind.CurlyBracketClose)
            {
                var elementName = tokens.PopExpected(TokenKind.Word).Value;
                var next = tokens.Pop();

                switch (next.Kind)
                {
                    case TokenKind.AssigmnentOperator:
                        var indexStr = tokens.PopExpected(TokenKind.Number).Value;
                        var index = int.Parse(indexStr);
                        if (index < currentIndex)
                            throw new Exception("Invalid enum element index for " + enumName + "." + elementName + "! Must be >= " + currentIndex);
                        currentIndex = index;
                        next = tokens.PopExpected(TokenKind.Comma, TokenKind.CurlyBracketClose);
                        break;

                    case TokenKind.Comma:
                    case TokenKind.CurlyBracketClose:
                        //Nothing to do here, but also no error
                        break;

                    default:
                        throw new Exception("Unexpected token in enum declaration: " + next);
                }

                elements.Add(AstItem.EnumElement(elementName, currentIndex));
                currentIndex += 1;
                current = next;
            }

            return AstItem.EnumDecl(enumName, elements);
        }

        private AstItem ReadFuncDecl(TokenIterator tokens)
        {
            //Skip "fn"
            tokens.Pop();

            var next = tokens.Current();
            if (next.Kind == TokenKind.Ext)
                return ReadExternFuncDecl(tokens);

            var name = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.RoundBracketOpen);

            var parameters = ReadFuncParameters(tokens);

            DataType returnType = DataType.VOID;

            next = tokens.Pop();
            if (next.Kind == TokenKind.TypeOperator)
            {
                returnType = ReadDataType(tokens);
                next = tokens.Pop();
            }

            if (next.Kind != TokenKind.CurlyBracketOpen)
                throw new Exception("Expected ':' or '{', found " + next);

            var statements = ReadStatements(tokens);

            tokens.PopExpected(TokenKind.CurlyBracketClose);

            return AstItem.FunctionDecl(name.Value, returnType, parameters, statements);
        }

        private AstItem ReadExternFuncDecl(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.Ext);
            tokens.PopExpected(TokenKind.RoundBracketOpen);

            var libFnName = tokens.PopExpected(TokenKind.String);
            tokens.PopExpected(TokenKind.Comma);
            var libName = tokens.PopExpected(TokenKind.String);

            tokens.PopExpected(TokenKind.RoundBracketClose);

            var name = tokens.PopExpected(TokenKind.Word);

            tokens.PopExpected(TokenKind.RoundBracketOpen);

            var parameters = ReadFuncParameters(tokens);

            var next = tokens.Pop();
            DataType returnType = DataType.VOID;

            if (next.Kind == TokenKind.TypeOperator)
            {
                returnType = ReadDataType(tokens);
                tokens.PopExpected(TokenKind.StatementTerminator);
            }

            return AstItem.ExternFunctionDecl(name.Value, returnType, parameters, libFnName.Value, libName.Value);
        }

        private List<AstItem> ReadFuncParameters(TokenIterator tokens)
        {
            var result = new List<AstItem>();
            var current = tokens.Pop();
            while (current.Kind != TokenKind.RoundBracketClose)
            {
                var name = current;
                if (name.Kind != TokenKind.Word)
                    throw new Exception("Expected parameter name, found " + name);

                tokens.PopExpected(TokenKind.TypeOperator);

                var dataType = ReadDataType(tokens);

                result.Add(AstItem.Parameter(name.Value, dataType));

                current = tokens.Pop();
                if (current.Kind == TokenKind.Comma)
                    current = tokens.Pop();
            }
            return result;
        }

        private List<AstItem> ReadStatements(TokenIterator tokens)
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

        private AstItem ReadStatement(TokenIterator tokens)
        {
            AstItem result = null;

            var token = tokens.Current();
            tokens.StartCapture();

            switch (token.Kind)
            {
                case TokenKind.Let:
                    result = ReadVarDecl(tokens);
                    break;

                case TokenKind.Ret:
                    //Pop "ret"
                    tokens.Pop();

                    var current = tokens.Current();
                    DataType valueType = DataType.VOID;
                    AstItem valueExpression = null;
                    if (current.Kind != TokenKind.StatementTerminator)
                    {
                        valueExpression = ReadExpression(tokens, TokenKind.StatementTerminator);
                        valueType = valueExpression.DataType;
                    }
                    result = AstItem.Return(valueType, valueExpression);
                    tokens.PopExpected(TokenKind.StatementTerminator);
                    break;

                case TokenKind.Word:
                    var next = tokens.Next();
                    if (next.Kind == TokenKind.RoundBracketOpen)
                    {
                        result = ReadFuncCall(tokens);
                        tokens.PopExpected(TokenKind.StatementTerminator);
                    }
                    else if (next.Kind == TokenKind.AssigmnentOperator)
                        result = ReadVariableAssignment(tokens);
                    else if (next.Kind == TokenKind.SquareBracketOpen)
                        result = ReadIndexerAssignment(tokens);
                    else
                        throw new Exception("Unexpected token after identifier '" + token.Value + "'. Expected '=' or '(', found: " + next);
                    break;

                case TokenKind.If:
                    result = ReadIfStatement(tokens);
                    break;

                case TokenKind.Del:
                    result = ReadDelStatement(tokens);
                    tokens.PopExpected(TokenKind.StatementTerminator);
                    break;

                case TokenKind.ExpressionOperator:
                    if (token.Value == "*")
                    {
                        result = ReadPointerAssignment(tokens);
                    }
                    else
                        throw new Exception("Unexpected token. Expected 'let', 'ret', '}' or identifier, found: " + token);

                    break;

                case TokenKind.For:
                    result = ReadForLoop(tokens);
                    break;

                case TokenKind.While:
                    result = ReadWhileLoop(tokens);
                    break;

                case TokenKind.Break:
                    result = ReadBreak(tokens);
                    break;

                case TokenKind.CurlyBracketClose:
                    //End of function
                    return null;

                default:
                    throw new Exception("Unexpected token. Expected 'let', 'ret', '}' or identifier, found: " + token);
            }

            var lineTokens = tokens.GetCapture();
            if (lineTokens.Count > 0)
                result.SourceLine = String.Join(" ", lineTokens.ConvertAll<string>((a) => a.Value));

            return result;
        }

        private static AstItem ReadBreak(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.Break);
            tokens.PopExpected(TokenKind.StatementTerminator);
            return AstItem.Break();
        }

        private AstItem ReadDelStatement(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.Del);
            var varName = tokens.PopExpected(TokenKind.Word);
            return AstItem.DelPointer(varName.Value);
        }

        private AstItem ReadIfStatement(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.If);
            var expression = ReadExpression(tokens, TokenKind.CurlyBracketOpen);
            tokens.PopExpected(TokenKind.CurlyBracketOpen);
            var statements = ReadStatements(tokens);
            tokens.PopExpected(TokenKind.CurlyBracketClose);

            List<AstItem> elseStatements = null;

            var current = tokens.Current();
            if (current.Kind == TokenKind.Else)
            {
                tokens.PopExpected(TokenKind.Else);
                tokens.PopExpected(TokenKind.CurlyBracketOpen);
                elseStatements = ReadStatements(tokens);
                tokens.PopExpected(TokenKind.CurlyBracketClose);
            }

            return AstItem.IfStatement(expression, statements, elseStatements);
        }

        private AstItem ReadForLoop(TokenIterator tokens)
        {
            //for i in 0..5 [inc 1] {...}
            tokens.PopExpected(TokenKind.For);
            var varName = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.In);
            var startExpression = ReadExpression(tokens, TokenKind.To);
            tokens.PopExpected(TokenKind.To);
            var endExpression = ReadExpression(tokens, new List<TokenKind>() { TokenKind.CurlyBracketOpen, TokenKind.Inc });

            var next = tokens.Pop();

            AstItem incExpression = AstItem.Immediate("1");
            if (next.Kind == TokenKind.Inc)
            {
                incExpression = ReadExpression(tokens, TokenKind.CurlyBracketOpen);
                tokens.PopExpected(TokenKind.CurlyBracketOpen);
            }
            else
                Assert.True(next.Kind == TokenKind.CurlyBracketOpen, "Expected 'inc' or '{', got: " + next);

            var statements = ReadStatements(tokens);
            tokens.PopExpected(TokenKind.CurlyBracketClose);
            return AstItem.ForLoop(varName.Value, startExpression, endExpression, incExpression, statements);
        }

        private AstItem ReadWhileLoop(TokenIterator tokens)
        {
            //while a > 5 {...}
            tokens.PopExpected(TokenKind.While);
            var whileExpression = ReadExpression(tokens, TokenKind.CurlyBracketOpen);
            tokens.PopExpected(TokenKind.CurlyBracketOpen);
            var statements = ReadStatements(tokens);
            tokens.PopExpected(TokenKind.CurlyBracketClose);
            return AstItem.WhileLoop(whileExpression, statements);
        }

        private AstItem ReadFuncCall(TokenIterator tokens)
        {
            var name = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.RoundBracketOpen);

            var paramValues = ReadTokenList(tokens, TokenKind.Comma, TokenKind.RoundBracketClose);
            var paramExpressions = new List<AstItem>(paramValues.Count);
            foreach (var valueTokens in paramValues)
            {
                var expression = ReadExpression(new TokenIterator(valueTokens), null);
                paramExpressions.Add(expression);
            }

            return AstItem.FunctionCall(name.Value, paramExpressions);
        }

        private List<List<Token>> ReadTokenList(TokenIterator tokens, TokenKind separator, TokenKind terminator)
        {
            var result = new List<List<Token>>();
            var expTokens = new List<Token>();

            var token = tokens.Pop();
            if (token.Kind == terminator)
                return result;

            while (token != null)
            {
                if (token.Kind == separator)
                {
                    result.Add(expTokens);
                    expTokens = new List<Token>();
                }
                else if (token.Kind == terminator)
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

        private AstItem ReadVarDecl(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.Let);
            var name = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.AssigmnentOperator);

            var expression = ReadExpression(tokens, TokenKind.StatementTerminator);

            tokens.PopExpected(TokenKind.StatementTerminator);

            return AstItem.VarDecl(name.Value, expression);
        }

        private AstItem ReadVariableAssignment(TokenIterator tokens)
        {
            var name = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.AssigmnentOperator);

            var expression = ReadExpression(tokens, TokenKind.StatementTerminator);

            tokens.PopExpected(TokenKind.StatementTerminator);

            return AstItem.VariableAssignment(name.Value, expression);
        }

        private AstItem ReadPointerAssignment(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.ExpressionOperator);
            var name = tokens.PopExpected(TokenKind.Word);
            tokens.PopExpected(TokenKind.AssigmnentOperator);

            var expression = ReadExpression(tokens, TokenKind.StatementTerminator);

            tokens.PopExpected(TokenKind.StatementTerminator);

            return AstItem.PointerAssignment(name.Value, expression);
        }

        private AstItem ReadIndexerAssignment(TokenIterator tokens)
        {
            var name = tokens.PopExpected(TokenKind.Word);
            
            tokens.PopExpected(TokenKind.SquareBracketOpen);
            var indexExpression = ReadExpression(tokens, TokenKind.SquareBracketClose);
            tokens.PopExpected(TokenKind.SquareBracketClose);

            tokens.PopExpected(TokenKind.AssigmnentOperator);

            var valueExpression = ReadExpression(tokens, TokenKind.StatementTerminator);

            tokens.PopExpected(TokenKind.StatementTerminator);

            return AstItem.PointerIndexAssignment(name.Value, indexExpression, valueExpression);
        }

        private DataType ReadDataType(TokenIterator tokens)
        {
            var typeName = tokens.PopExpected(TokenKind.Word);

            var name = typeName.Value;
            var result = DataType.GetAllValues().Find((t) => t.Name == name);

            if (result == null)
                throw new Exception("Unknown type: " + name + " at " + typeName);

            var token = tokens.Current();
            var isPointer = false;
            if (token != null && token.Kind == TokenKind.ExpressionOperator)
            {
                if (token.Value == "*")
                {
                    isPointer = true;
                    tokens.Pop();
                }
                else
                    throw new Exception("Expected data type, got: " + token);
            }

            if (isPointer)
            {
                name = name +  "*";
                var pointerType = DataType.GetAllValues().Find((t) => t.Name == name);

                if (pointerType == null)
                    pointerType = DataType.Pointer(result);

                result = pointerType;
            }

            return result;
        }

        private AstItem ReadExpression(TokenIterator tokens, TokenKind terminator)
        {
            var terminators = new List<TokenKind>() { terminator };
            return ReadExpression(tokens, terminators);
        }

        private AstItem ReadExpression(TokenIterator tokens, List<TokenKind> terminators)
        {
            var expTokens = new List<Token>();
            if (terminators == null)
            {
                expTokens.AddRange(tokens.ToList());
            }
            else
            {
                var tok = tokens.Current();
                while (tok != null && !terminators.Contains(tok.Kind))
                {
                    expTokens.Add(tok);
                    tokens.Step();
                    tok = tokens.Current();
                }
            }

            if (expTokens.Count == 0)
                throw new Exception("Expected expression, found " + tokens.Current());

            AstItem result = null;
            var tokenIter = new TokenIterator(expTokens);

            if (expTokens.Count == 1)
            {
                //Single immediate, vector or variable
                result = ReadSingleAstItem(tokenIter);
            }
            else
            {
                var token = tokenIter.Current();
                var next = tokenIter.Next();
                if (token.Kind == TokenKind.New)
                {
                    result = ReadNewPointer(tokenIter);
                }
                else
                {
                    //Math Expression
                    var expItemsInfix = new List<AstItem>();
                    var expectOperand = true;
                    while (token != null)
                    {
                        switch (token.Kind)
                        {
                            case TokenKind.Word:
                            case TokenKind.Number:
                            case TokenKind.True:
                            case TokenKind.False:
                            case TokenKind.VectorConstructor:
                                var operandItem = ReadSingleAstItem(tokenIter);
                                expItemsInfix.Add(operandItem);
                                expectOperand = false; //Next we want an operator
                                break;

                            case TokenKind.RoundBracketOpen:
                            case TokenKind.RoundBracketClose:
                                expItemsInfix.Add(AstItem.AsOperator(ParseOperator(token.Value)));
                                break;

                            case TokenKind.ExpressionOperator:
                                if (expectOperand)
                                {
                                    expItemsInfix.Add(AstItem.AsUnaryOperator(ParseUnaryOperator(token.Value)));
                                }
                                else
                                {
                                    expItemsInfix.Add(AstItem.AsOperator(ParseOperator(token.Value)));
                                    expectOperand = true; //Next we want an operand
                                }
                                break;

                            default:
                                throw new Exception("Unexpected expression token: " + token);
                        }

                        tokenIter.Step();
                        token = tokenIter.Current();
                    }

                    //Convert to postfix
                    if (expItemsInfix.Count > 1)
                        result = InfixToPostfix(expItemsInfix);
                    else
                        result = expItemsInfix[0];
                }
            }

            return result;
        }

        private AstItem ReadIndexAccess(TokenIterator tokenIter)
        {
            var symbol = tokenIter.PopExpected(TokenKind.Word);
            tokenIter.PopExpected(TokenKind.SquareBracketOpen);

            var valueExpression = ReadExpression(tokenIter, TokenKind.SquareBracketClose);

            tokenIter.PopExpected(TokenKind.SquareBracketClose);

            return AstItem.IndexAccess(symbol.Value, valueExpression);
        }

        private AstItem ReadNewPointer(TokenIterator tokens)
        {
            tokens.PopExpected(TokenKind.New);
            var subType = ReadDataType(tokens);

            var dataType = DataType.Pointer(subType);

            var amountStr = "1";
            var current = tokens.Current();
            if (current != null && current.Kind == TokenKind.RoundBracketOpen)
            {
                tokens.Pop();
                var amountToken = tokens.PopExpected(TokenKind.Number);
                amountStr = amountToken.Value;
                tokens.PopExpected(TokenKind.RoundBracketClose);
            }

            return AstItem.NewPointer(dataType, amountStr);
        }

        private AstItem ReadSingleAstItem(TokenIterator tokens)
        {
            AstItem result;
            var token = tokens.Current();

            if (token.Kind == TokenKind.Word)
            {
                var next = tokens.Next();
                if (next != null)
                {
                    if (next.Kind == TokenKind.RoundBracketOpen)
                    {
                        var dataType = DataType.FindByName(token.Value);
                        if (dataType != null && dataType.IsVector)
                        {
                            //Vector construction with specific vector type name, i.e. "vec4f(...)"
                            result = ReadVector(tokens);
                            tokens.StepBack();
                        }
                        else
                        {
                            result = ReadFuncCall(tokens);
                            tokens.StepBack();
                        }
                    }
                    else if (next.Kind == TokenKind.SquareBracketOpen)
                    {
                        result = ReadIndexAccess(tokens);
                        tokens.StepBack();
                    }
                    else
                    {
                        //Not sure what identifier is here. Could be type name, variable etc. Will be specified in next steps.
                        result = AstItem.AsIdentifier(token.Value);
                    }
                }
                else
                {
                    //Not sure what identifier is here. Could be type name, variable etc. Will be specified in next steps.
                    result = AstItem.AsIdentifier(token.Value);
                }
            }
            else if (token.Kind == TokenKind.VectorConstructor)
            {
                //Vector construction with the generic "vec(...)"
                result = ReadVector(tokens);
                tokens.StepBack();
            }
            else if (token.Kind == TokenKind.Number)
            {
                result = AstItem.Immediate(token.Value);
            }
            else if (token.Kind == TokenKind.True || token.Kind == TokenKind.False)
            {
                result = AstItem.Immediate(token.Value);
            }
            else
                throw new Exception("Unexpected token type in expression: " + token);

            return result;
        }

        private AstItem ReadVector(TokenIterator tokens)
        {
            var name = tokens.Pop();

            var bracket = tokens.Pop();
            if (bracket.Kind != TokenKind.RoundBracketOpen)
                throw new Exception("Unexcepted token after vector name! Expected '(', found: " + bracket);

            var vectorValues = new List<List<Token>>();
            var valueTokens = new List<Token>();
            var bracketCounter = 0;
            var token = tokens.Pop();
            while (token != null)
            {
                if (token.Kind == TokenKind.RoundBracketOpen)
                {
                    bracketCounter += 1;
                    valueTokens.Add(token);
                }
                else if (token.Kind == TokenKind.RoundBracketClose)
                {
                    if (bracketCounter <= 0)
                    {
                        vectorValues.Add(valueTokens);
                        break;
                    }
                    else
                    {
                        bracketCounter -= 1;
                        valueTokens.Add(token);
                    }
                }
                else if (token.Kind == TokenKind.Comma)
                {
                    vectorValues.Add(valueTokens);
                    valueTokens = new List<Token>();
                }
                else
                {
                    valueTokens.Add(token);
                }

                token = tokens.Pop();
            }

            var paramExpressions = new List<AstItem>(vectorValues.Count);
            foreach (var values in vectorValues)
            {
                var expression = ReadExpression(new TokenIterator(values), null);
                paramExpressions.Add(expression);
            }

            //Need to pass the name so SemanticAnalysis knows if "vec" or (i.e.) "vec4f" was used
            return AstItem.Vector(name.Value, paramExpressions);
        }

        private IBinaryOperator ParseOperator(string op)
        {
            var oper = Operator.Parse(op);
            if (oper == null)
                throw new Exception("Unsupported expression operator: " + op);

            return oper;
        }

        private IUnaryOperator ParseUnaryOperator(string op)
        {
            var oper = UnaryOperator.Parse(op);
            if (oper == null)
                throw new Exception("Unsupported unary operator: " + op);

            return oper;
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
                if (item.Kind == AstItemKind.Immediate || item.Kind == AstItemKind.Variable || item.Kind == AstItemKind.Vector || item.Kind == AstItemKind.FunctionCall || item.Kind == AstItemKind.Type || item.Kind == AstItemKind.Identifier || item.Kind == AstItemKind.IndexAccess)
                {
                    output.Add(item);
                }
                else if (item.Operator == Operator.ROUND_BRACKET_OPEN)
                {
                    stack.Push(item);
                }
                else if (item.Operator == Operator.ROUND_BRACKET_CLOSE)
                {
                    cbuffer = stack.Pop();
                    while (cbuffer.Operator != Operator.ROUND_BRACKET_OPEN)
                    {
                        output.Add(cbuffer);
                        cbuffer = stack.Pop();
                    }
                }
                else if (item.Kind == AstItemKind.BinaryOperator || item.Kind == AstItemKind.UnaryOperator)
                {
                    if (stack.Count != 0 && Predecessor(stack.Peek(), item))
                    {
                        cbuffer = stack.Pop();
                        while (Predecessor(cbuffer, item))
                        {
                            output.Add(cbuffer);

                            if (stack.Count == 0)
                                break;

                            cbuffer = stack.Peek();
                            // With unary operators it is now possible to find a round bracket here, which must not be popped!
                            if (cbuffer.Kind == AstItemKind.BinaryOperator && cbuffer.Operator == Operator.ROUND_BRACKET_OPEN)
                                break;
                            else
                                stack.Pop();
                        }
                        stack.Push(item);
                    }
                    else
                        stack.Push(item);
                }
                else
                    throw new Exception("Unexpected Ast item in expression: " + item);
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
            return firstOperator.Operator.Precedence >= secondOperator.Operator.Precedence;
        }

    }
}