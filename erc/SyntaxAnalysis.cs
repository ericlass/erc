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
            List<AstItem> parameters = null;
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
            AstItem result = null;

            var token = tokens.Current();
            tokens.StartCapture();

            switch (token.TokenType)
            {
                case TokenType.Let:
                    result = ReadVarDecl(tokens);
                    break;

                case TokenType.Ret:
                    //Pop "ret"
                    tokens.Pop();

                    var value = ReadExpression(tokens, TokenType.StatementTerminator);
                    result = AstItem.Return(value.DataType, value);

                    //Pop ";"
                    tokens.Pop();
                    break;

                case TokenType.Word:
                    var next = tokens.Next();
                    if (next.TokenType == TokenType.RoundBracketOpen)
                    {
                        result = ReadFuncCall(tokens);
                        //Pop ";"
                        tokens.Pop();
                    }
                    else if (next.TokenType == TokenType.AssigmnentOperator)
                        result = ReadAssignment(tokens);
                    else
                        throw new Exception("Unexpected token after identifier '" + token.Value + "'. Expected '=' or '(', found: " + next);
                    break;

                case TokenType.CurlyBracketClose:
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

        private AstItem ReadFuncCall(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();

            var bracket = tokens.Pop();
            if (bracket.TokenType != TokenType.RoundBracketOpen)
                throw new Exception("Unexcepted token after function name! Expected '(', found: " + bracket);

            var paramValues = ReadTokenList(tokens, TokenType.Comma, TokenType.RoundBracketClose);
            var paramExpressions = new List<AstItem>(paramValues.Count);
            foreach (var valueTokens in paramValues)
            {
                var expression = ReadExpression(new SimpleIterator<Token>(valueTokens), null);
                paramExpressions.Add(expression);
            }

            return AstItem.FunctionCall(name.Value, paramExpressions);
        }

        private List<List<Token>> ReadTokenList(SimpleIterator<Token> tokens, TokenType separator, TokenType terminator)
        {
            var result = new List<List<Token>>();
            var expTokens = new List<Token>();

            var token = tokens.Pop();
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

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator)
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens, TokenType.StatementTerminator);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.VarDecl(name.Value, expression);
        }

        private AstItem ReadAssignment(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();
            if (name.TokenType != TokenType.Word)
                throw new Exception("Expected identifier, found " + name);

            var op = tokens.Pop();
            if (op.TokenType != TokenType.AssigmnentOperator)
                throw new Exception("Expected assignment operator, found " + name);

            var expression = ReadExpression(tokens, TokenType.StatementTerminator);

            var terminator = tokens.Pop();
            if (terminator.TokenType != TokenType.StatementTerminator)
                throw new Exception("Expected statement terminator, found " + name);

            return AstItem.Assignment(name.Value, expression);
        }

        private DataType ReadDataType(SimpleIterator<Token> tokens)
        {
            var typeName = tokens.Pop();
            if (typeName.TokenType != TokenType.Word)
                throw new Exception("Expected data type, found " + typeName);

            var name = typeName.Value;
            var result = DataType.GetAllValues().Find((t) => t.Name == name);

            if (result == null)
                throw new Exception("Unknown type: " + name);

            return result;
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
                var token = tokenIter.Current();
                while (token != null)
                {
                    switch (token.TokenType)
                    {
                        case TokenType.Word:
                        case TokenType.Number:
                        case TokenType.True:
                        case TokenType.False:
                        case TokenType.VectorConstructor:
                            var operandItem = ReadSingleAstItem(tokenIter);
                            expItemsInfix.Add(operandItem);
                            break;

                        case TokenType.ExpressionOperator:
                        case TokenType.RoundBracketOpen:
                        case TokenType.RoundBracketClose:
                            expItemsInfix.Add(AstItem.AsOperator(ParseOperator(token.Value)));
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
                else
                {
                    result = AstItem.Variable(token.Value);
                }
            }
            else if (token.TokenType == TokenType.VectorConstructor)
            {
                //Vector construction with the generic "vec(...)"
                result = ReadVector(tokens);
                tokens.StepBack();
            }
            else if (token.TokenType == TokenType.Number)
            {
                result = AstItem.Immediate(token.Value);
            }
            else if (token.TokenType == TokenType.True || token.TokenType == TokenType.False)
            {
                result = AstItem.Immediate(token.Value);
            }
            else
                throw new Exception("Unexpected token type in expression: " + token);

            return result;
        }

        private AstItem ReadVector(SimpleIterator<Token> tokens)
        {
            var name = tokens.Pop();

            var bracket = tokens.Pop();
            if (bracket.TokenType != TokenType.RoundBracketOpen)
                throw new Exception("Unexcepted token after vector name! Expected '(', found: " + bracket);

            var vectorValues = new List<List<Token>>();
            var valueTokens = new List<Token>();
            var bracketCounter = 0;
            var token = tokens.Pop();
            while (token != null)
            {
                if (token.TokenType == TokenType.RoundBracketOpen)
                {
                    bracketCounter += 1;
                    valueTokens.Add(token);
                }
                else if (token.TokenType == TokenType.RoundBracketClose)
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
                else if (token.TokenType == TokenType.Comma)
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
                var expression = ReadExpression(new SimpleIterator<Token>(values), null);
                paramExpressions.Add(expression);
            }

            //Need to pass the name so SemanticAnalysis knows if "vec" or (i.e.) "vec4f" was used
            return AstItem.Vector(name.Value, paramExpressions);
        }

        private IOperator ParseOperator(string op)
        {
            var oper = Operator.Parse(op);
            if (oper == null)
                throw new Exception("Unsupported math operator: " + op);

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
                if (item.Kind == AstItemKind.Immediate || item.Kind == AstItemKind.Variable || item.Kind == AstItemKind.Vector || item.Kind == AstItemKind.FunctionCall)
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
                else if (item.Kind == AstItemKind.Operator)
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