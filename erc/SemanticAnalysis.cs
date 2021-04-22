using System;
using System.Collections.Generic;
using System.Globalization;

namespace erc
{
    public class SemanticAnalysis
    {
        private const string FakeEndLabelName = "scope-end-label";
        private HashSet<char> _numberSuffixChars = new HashSet<char> { 'b', 'w', 'd', 'q', 'u', 'f' }; //d is used for both 32 bit integer and float

        private CompilerContext _context;
        private AstOptimizer _optimizer = new AstOptimizer();

        public SemanticAnalysis()
        {
        }

        public void Analyze(CompilerContext context)
        {
            _context = context;

            AddAllFunctionsAndTypesToScope(_context.AST);
            Check(_context.AST);
            _optimizer.Optimize(_context.AST);
        }

        private void AddAllFunctionsAndTypesToScope(AstItem item)
        {
            foreach (var topItem in item.Children)
            {
                if (topItem.Kind == AstItemKind.FunctionDecl || topItem.Kind == AstItemKind.ExternFunctionDecl)
                {
                    string externalName = null;
                    if (topItem.Kind == AstItemKind.ExternFunctionDecl)
                        externalName = topItem.Value2 as string;

                    var parameters = topItem.Children[0].Children;
                    var funcParams = parameters.ConvertAll((p) => new Symbol(p.Identifier, SymbolKind.Parameter, p.DataType));

                    var functionIsVariadic = false;

                    var variadicParams = funcParams.FindAll((p) => p.DataType.Kind == DataTypeKind.VARS);
                    if (variadicParams.Count > 1)
                        throw new Exception("A function can only have one variadic parameter defined, '" + topItem.Identifier + "' has multiple: " + String.Join(", ", funcParams));

                    if (variadicParams.Count == 1)
                    {
                        var lastParam = funcParams[funcParams.Count - 1];
                        if (lastParam.DataType.Kind != DataTypeKind.VARS)
                            throw new Exception("Function '" + topItem.Identifier + "' has variadic parameter, but it is not the last one!");

                        functionIsVariadic = true;
                        //Remove the variadic parameters, the whole function will be marked as variadic
                        funcParams.RemoveAt(funcParams.Count - 1);
                    }

                    var function = new Function(topItem.Identifier, topItem.DataType, funcParams, externalName);
                    function.IsExtern = topItem.Kind == AstItemKind.ExternFunctionDecl;
                    function.IsVariadic = functionIsVariadic;
                    //This fails if function with same name was already declared
                    _context.AddFunction(function);
                }
                else if (topItem.Kind == AstItemKind.EnumDecl)
                {
                    var enumName = topItem.Identifier;

                    var existing = DataType.FindByName(enumName);
                    if (existing != null)
                        throw new Exception("Enum with name '" + enumName + "' already declared!");

                    var elements = topItem.Children.ConvertAll((e) => new EnumElement(e.Identifier, (int)e.Value));
                    DataType.Enum(enumName, elements);
                }
            }
        }

        private void Check(AstItem item)
        {
            foreach (var child in item.Children)
            {
                switch (child.Kind)
                {
                    case AstItemKind.FunctionDecl:
                    case AstItemKind.ExternFunctionDecl:
                        CheckFunction(child);
                        break;

                    case AstItemKind.EnumDecl:
                        CheckEnumDecl(child);
                        break;

                    default:
                        throw new Exception("Unexpected AST item at top level: " + child);
                }                    
            }
        }

        private void CheckEnumDecl(AstItem item)
        {
            var enumName = item.Identifier;
            var enumType = DataType.FindByName(enumName);
            Assert.True(enumType != null, "Enum type not found: " + enumName);
            Assert.DataTypeKind(enumType.Kind, DataTypeKind.ENUM, "Invalid enum data type");
            Assert.True(enumType.EnumElements != null && enumType.EnumElements.Count > 0, "Enum does not have any elements: " + enumName);

            //Check for duplicate element names (ignore case) and indexes
            var elementSet = new HashSet<string>();
            var indexSet = new HashSet<int>();
            foreach (var element in enumType.EnumElements)
            {
                var lowerName = element.Name.ToLower();
                if (elementSet.Contains(lowerName))
                    throw new Exception("Duplicate enum element name " + element.Name + " in enum " + enumName);

                if (indexSet.Contains(element.Index))
                    throw new Exception("Duplicate enum element index " + element.Index + " for element " + element.Name + " in enum " + enumName);

                elementSet.Add(lowerName);
                indexSet.Add(element.Index);
            }
        }

        private void CheckFunction(AstItem item)
        {
            if (item.Kind != AstItemKind.FunctionDecl && item.Kind != AstItemKind.ExternFunctionDecl)
                throw new Exception("Expected function declaration, got " + item);

            var currentFunction = _context.GetFunction(item.Identifier);
            if (currentFunction == null)
                throw new Exception("Function not found in scope: " + item);

            //Nothing more to check for external functions
            if (currentFunction.IsExtern)
                return;

            _context.EnterFunction(currentFunction);
            _context.EnterBlock();

            var statements = item.Children[1].Children;
            foreach (var statement in statements)
            {
                CheckStatement(statement);
            }

            //Check that function that should return a value has a return statement
            if (currentFunction.ReturnType != null && currentFunction.ReturnType != DataType.VOID)
            {
                var returnIndex = -1;
                for (int i = statements.Count - 1; i >= 0; i--)
                {
                    if (statements[i].Kind == AstItemKind.Return)
                    {
                        returnIndex = i;
                        break;
                    }
                }

                if (returnIndex < 0)
                {
                    throw new Exception("Return statement required, but none found in function: " + currentFunction);
                }
                else if (returnIndex < statements.Count - 1)
                {
                    _context.Logger.Warn("Statements found after return statement, will not be compiled. In function: " + currentFunction);
                    //TODO: Those can also be removed so no code is generated for them.
                }
            }

            _context.LeaveBlock();
            _context.LeaveFunction();
        }

        private void CheckStatement(AstItem item)
        {
            if (item.Kind == AstItemKind.VarDecl)
            {
                CheckVarDecl(item);
            }
            else if (item.Kind == AstItemKind.Assignment)
            {
                CheckAssignment(item);
            }
            else if (item.Kind == AstItemKind.FunctionCall)
            {
                CheckFunctionCall(item);
            }
            else if (item.Kind == AstItemKind.Return)
            {
                CheckReturnStatement(item);
            }
            else if (item.Kind == AstItemKind.If)
            {
                CheckIfStatement(item);
            }
            else if (item.Kind == AstItemKind.For)
            {
                CheckForLoop(item);
            }
            else if (item.Kind == AstItemKind.While)
            {
                CheckWhileLoop(item);
            }
            else if (item.Kind == AstItemKind.Break)
            {
                CheckBreakStatement();
            }
            else if (item.Kind == AstItemKind.DelPointer)
            {
                CheckDelPointer(item);
            }
            else
                throw new Exception("Unknown statement: " + item);
        }

        private void CheckDelPointer(AstItem item)
        {
            var variable = _context.GetSymbol(item.Identifier);
            if (variable == null)
                throw new Exception("Undeclared variable: '" + item.Identifier + "' at: " + item);

            var isHeapArray = variable.DataType.Kind == DataTypeKind.ARRAY && variable.DataType.MemoryRegion == MemoryRegion.Heap;
            if (variable.DataType.Kind != DataTypeKind.POINTER && !isHeapArray)
                throw new Exception("Can only delete pointer or heap-array data type: " + variable.DataType + " at: " + item);

            //TODO: Check that the pointer is one that was created with "new" and not some other self-created one
        }

        private void CheckIfStatement(AstItem item)
        {
            var dataType = CheckExpression(item.Children[0]);
            if (dataType != DataType.BOOL)
                throw new Exception("Expression for if statement must return bool, got: " + dataType);

            //if block
            _context.EnterBlock();

            var statements = item.Children[1].Children;
            statements.ForEach((s) => CheckStatement(s));

            _context.LeaveBlock();

            //else block
            var elseStatements = item.Children[2];
            if (elseStatements != null)
            {
                _context.EnterBlock();
                elseStatements.Children.ForEach((s) => CheckStatement(s));
                _context.LeaveBlock();
            }
        }

        private void CheckForLoop(AstItem item)
        {
            var varName = item.Identifier;
            var startExpression = item.Children[0];
            var endExpression = item.Children[1];
            var incExpression = item.Children[2];
            var statements = item.Children[3];

            CheckExpression(startExpression);
            Assert.DataTypeKind(startExpression.DataType.Kind, DataTypeKind.I64, "Invalid data type for 'for' loop start expression");

            CheckExpression(endExpression);
            Assert.DataTypeKind(endExpression.DataType.Kind, DataTypeKind.I64, "Invalid data type for 'for' loop end expression");

            CheckExpression(incExpression);
            Assert.DataTypeKind(incExpression.DataType.Kind, DataTypeKind.I64, "Invalid data type for 'for' loop increment expression");

            _context.EnterBlock(FakeEndLabelName);
            _context.AddVariable(new Symbol(varName, SymbolKind.Variable, DataType.I64));

            statements.Children.ForEach((s) => CheckStatement(s));

            _context.LeaveBlock();
        }

        private void CheckWhileLoop(AstItem item)
        {
            var whileExpression = item.Children[0];
            var statements = item.Children[1];

            CheckExpression(whileExpression);
            Assert.DataTypeKind(whileExpression.DataType.Kind, DataTypeKind.BOOL, "Invalid data type for 'while' loop expression");

            _context.EnterBlock(FakeEndLabelName);
            statements.Children.ForEach((s) => CheckStatement(s));
            _context.LeaveBlock();
        }

        private void CheckBreakStatement()
        {
            Assert.True(_context.GetCurrentScopeEndLabel() != null, "break statement is not inside any breakable scope!");
        }

        private void CheckReturnStatement(AstItem item)
        {
            var valueExpression = item.Children[0];
            var valueType = DataType.VOID;

            if (valueExpression != null)
            {
                valueType = CheckExpression(valueExpression);

                if (valueType.Kind == DataTypeKind.ARRAY && valueType.MemoryRegion == MemoryRegion.Stack)
                    throw new Exception("Cannot return references to stack memory! The stack is deleted when the function returns! In:  " + item);

                if (valueType != _context.CurrentFunction.ReturnType)
                    throw new Exception("Invalid return data type! Expected " + _context.CurrentFunction.ReturnType + ", found " + valueType);
            }

            item.DataType = valueType;
        }

        private void CheckAssignment(AstItem item)
        {
            var target = item.Children[0];

            var variable = _context.GetSymbol(target.Identifier);
            Assert.True(variable != null, "Variable not declared: " + item);
            Assert.True(variable.IsAssignable, "Cannot assign to symbol: " + variable);

            item.DataType = CheckExpression(item.Children[1]);
            target.DataType = item.DataType;

            switch (target.Kind)
            {
                case AstItemKind.Variable:
                    Assert.True(variable.DataType.Equals(item.DataType), "Cannot assign value of type " + item.DataType + " to variable " + variable);
                    break;

                case AstItemKind.PointerDeref:
                    Assert.DataTypeKind(variable.DataType.Kind, DataTypeKind.POINTER, "Invalid data type for pointer dereference");
                    Assert.True(variable.DataType.ElementType.Equals(item.DataType), "Cannot assign value of type " + item.DataType + " to dereferenced pointer type " + variable.DataType);
                    break;

                case AstItemKind.IndexAccess:
                    target.DataType = CheckIndexAccess(target);
                    Assert.True(variable.DataType.ElementType.Equals(item.DataType), "Cannot assign value of type " + item.DataType + " to index access of type " + variable.DataType);
                    break;

                default:
                    throw new Exception("Unsupported assignment target: " + target);
            }
        }

        private void CheckVarDecl(AstItem item)
        {
            var variable = _context.GetSymbol(item.Identifier);
            if (variable != null)
                throw new Exception("Variable already declared: " + item);

            var dataType = CheckExpression(item.Children[0]);
            item.DataType = dataType;

            variable = new Symbol(item.Identifier, SymbolKind.Variable, dataType);
            _context.AddVariable(variable);
        }

        private DataType CheckExpression(AstItem expression)
        {
            switch (expression.Kind)
            {
                case AstItemKind.Immediate:
                    CheckImmediate(expression);
                    break;
                case AstItemKind.Vector:
                    CheckVector(expression);
                    break;
                case AstItemKind.Variable:
                    CheckVariable(expression);
                    break;
                case AstItemKind.FunctionCall:
                    CheckFunctionCall(expression);
                    break;
                case AstItemKind.Expression:
                    CheckExpressionItem(expression);
                    break;
                case AstItemKind.NewRawPointer:
                    CheckNewPointer(expression);
                    break;
                case AstItemKind.IndexAccess:
                    CheckIndexAccess(expression);
                    break;
                case AstItemKind.Identifier:
                    CheckIdentifier(expression);
                    break;
                case AstItemKind.CharLiteral:
                    CheckChar(expression);
                    break;
                case AstItemKind.NewStackArray:
                    CheckNewStackArray(expression);
                    break;
                case AstItemKind.NewHeapArray:
                    CheckNewHeapArray(expression);
                    break;
                default:
                    throw new Exception("Unsupported expression item: " + expression);
            }

            if (expression.DataType == null)
                throw new Exception("Could not determine data type for expression: " + expression);

            return expression.DataType;
        }

        private long? CheckValueArray(AstItem expression, MemoryRegion region)
        {
            //Check array is not empty
            Assert.True(expression.Children.Count > 0, "Empty arrays are not allowed: " + expression);

            //Check value expressions
            DataType valueType = null;
            foreach (var valueExpression in expression.Children)
            {
                var currentType = CheckExpression(valueExpression);
                if (valueType == null)
                    valueType = currentType;
                else
                    Assert.True(valueType.Equals(currentType), "Mixed types are not allowed in array definitions! First type: " + valueType + ", current Type: " + currentType);

                valueExpression.DataType = currentType;
            }

            var length = (long)expression.Children.Count;
            var arraySize = DataType.GetArrayByteSize(valueType, expression.Children.Count);
            Assert.True(arraySize <= 1024, "Arrays larger than 1KB should not go on the stack, put it on the heap instead! In: " + expression);

            expression.DataType = DataType.Array(valueType, region);

            return length;
        }

        private long? CheckSizedArray(AstItem expression, MemoryRegion region)
        {
            var value = expression.Children[0];
            var numItems = expression.Children[1];
            
            var valueType = CheckExpression(value);
            //No checks on value type, can be everything atm

            long? length = null;
            DataType numItemsType;
            if (numItems.Kind == AstItemKind.Immediate)
            {
                //TODO: Possibility to use defined constant as size, which is not "immediate"
                //Handle immediate values specifically so they always are U64
                numItemsType = DataType.U64;
                numItems.DataType = numItemsType;
                numItems.Value = ParseNumber((string)numItems.Value, numItemsType);
                length = (long)numItems.Value;
            }
            else
                numItemsType = CheckExpression(numItems);

            Assert.DataTypeKind(numItemsType.Kind, DataTypeKind.U64, "Size for sized arrays must be U64!");

            expression.DataType = DataType.Array(valueType, region);

            return length;
        }

        private void CheckNewStackArray(AstItem expression)
        {
            var arrayDefinition = expression.Children[0];
            var constArrayLength = CheckArrayDefinition(arrayDefinition, MemoryRegion.Stack);

            Assert.True(constArrayLength != null, "Stack located arrays must have a constant size known at compile time! Use heap arrays if size is only known at runtime.");

            var arraySize = DataType.GetArrayByteSize(arrayDefinition.DataType.ElementType, (long)constArrayLength);
            Assert.True(arraySize <= 1024, "Arrays larger than 1KB should not go on the stack, put it on the heap instead! In: " + expression);

            expression.DataType = arrayDefinition.DataType;
        }

        private void CheckNewHeapArray(AstItem expression)
        {
            var arrayDefinition = expression.Children[0];
            CheckArrayDefinition(arrayDefinition, MemoryRegion.Heap);

            expression.DataType = arrayDefinition.DataType;
        }

        private long? CheckArrayDefinition(AstItem expression, MemoryRegion region)
        {
            switch (expression.Kind)
            {
                case AstItemKind.ValueArrayDefinition:
                    return CheckValueArray(expression, region);

                case AstItemKind.SizedArrayDefinition:
                    return CheckSizedArray(expression, region);

                default:
                    throw new Exception("Invalid kind of AST item given! Expected: ValueArrayDefinition or SizedArrayDefinition, given: " + expression);
            }
        }

        private void CheckChar(AstItem expression)
        {
            var valueStr = (string)expression.Value;
            Assert.Count(valueStr.Length, 1, "Invalid length for char literal");

            expression.DataType = DataType.CHAR8;
        }

        private void CheckIdentifier(AstItem expression)
        {
            //Convert identifier to specific node, if possible
            var itemName = expression.Identifier;
            var dataType = DataType.FindByName(itemName);
            if (dataType != null)
            {
                //Item is a data type reference
                expression.DataType = dataType;
                expression.Kind = AstItemKind.Type;
                return;
            }

            var variable = _context.GetSymbol(itemName);
            if (variable != null)
            {
                //Item is a variable reference
                expression.Kind = AstItemKind.Variable;
                expression.Identifier = variable.Name;
                expression.DataType = variable.DataType;
                return;
            }

            //Member names, like enum values, stay identifier nodes and are handled specifically later
            expression.DataType = DataType.VOID;
        }

        private DataType CheckIndexAccess(AstItem expression)
        {
            var symbol = _context.RequireSymbol(expression.Identifier);
            Assert.True(symbol.DataType.Kind == DataTypeKind.POINTER || symbol.DataType.Kind == DataTypeKind.ARRAY, "Invalid data type for index access! Can be pointer or array, given: " + symbol.DataType);

            var indexExpression = expression.Children[0];
            var indexExpType = CheckExpression(indexExpression);
            
            if (indexExpression.Kind == AstItemKind.Immediate)
            {
                var index = (long)indexExpression.Value;
                if (index < 0)
                    throw new Exception("Cannot use negativ index values for index access!");
            }
            else
            {
                if (indexExpType.Group != DataTypeGroup.ScalarInteger || indexExpType.IsSigned)
                    throw new Exception("Index for index access must by unsigned integer type, got: " + indexExpType);
            }

            expression.DataType = symbol.DataType.ElementType;
            return symbol.DataType.ElementType;
        }

        private void CheckNewPointer(AstItem expression)
        {
            if (expression.DataType.Kind != DataTypeKind.POINTER)
                throw new Exception("Datatype for new pointer node must be reference! Found in: " + expression);

            DataType elementType = expression.DataType.ElementType;
            if (elementType == null)
                throw new Exception("Pointer type must have element type != null! Found in: " + expression);

            if (elementType.Kind == DataTypeKind.POINTER)
                throw new Exception("Cannot have pointer to pointer! Found in: " + expression);

            var amountExpression = expression.Children[0];
            DataType amountDataType;
            if (amountExpression.Kind == AstItemKind.Immediate)
            {
                //Handle immediate values specifically so they always are U64
                amountDataType = DataType.U64;
                amountExpression.DataType = amountDataType;
                amountExpression.Value = ParseNumber((string)amountExpression.Value, amountDataType);
            }
            else
                amountDataType = CheckExpression(amountExpression);

            if (amountDataType.Group != DataTypeGroup.ScalarInteger)
                throw new Exception("Amount value for new pointer must be integer type! Given: " + amountDataType);
        }

        private void CheckExpressionItem(AstItem expression)
        {
            //Create copy of list so original is not modified
            var terms = new List<AstItem>(expression.Children);

            DataType expressionFinalType = null;
            for (int i = 0; i < terms.Count; i++)
            {
                var item = terms[i];
                if (item.Kind == AstItemKind.BinaryOperator)
                {
                    var operand1 = terms[i - 2];
                    var operand2 = terms[i - 1];

                    //Make sure operand1 has a type
                    if (operand1.Kind != AstItemKind.BinaryOperator && operand1.Kind != AstItemKind.UnaryOperator)
                        CheckExpression(operand1);

                    //Make sure operand2 has a type
                    if (operand2.Kind != AstItemKind.BinaryOperator && operand2.Kind != AstItemKind.UnaryOperator)
                        CheckExpression(operand2);

                    //Check if operand types are valid
                    item.BinaryOperator.ValidateOperands(operand1, operand2);

                    //Set data type on operator
                    item.DataType = item.BinaryOperator.GetReturnType(operand1, operand2);

                    //Overwritten in every iteration. The last operator defines the final data type.
                    expressionFinalType = item.DataType;

                    //Remove the two operands from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 2);
                    terms.RemoveAt(i - 2);

                    //-2 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 2;
                }
                else if (item.Kind == AstItemKind.UnaryOperator)
                {
                    var operand = terms[i - 1];

                    //Make sure operand has a type
                    if (operand.Kind != AstItemKind.BinaryOperator && operand.Kind != AstItemKind.UnaryOperator)
                        CheckExpression(operand);

                    //Check if operand type is valid
                    item.UnaryOperator.ValidateOperand(operand);

                    //Set data type on operator
                    item.DataType = item.UnaryOperator.GetReturnType(operand);

                    //Overwritten in every iteration. The last operator defines the final data type.
                    expressionFinalType = item.DataType;

                    //Remove the operand from the expression, do not remove the current operator
                    //as it is required to know later that a value must be popped from stack
                    terms.RemoveAt(i - 1);

                    //-1 is correct because next iteration of loop will increment i
                    //anyways and it will then point to the next item in the expression.
                    i -= 1;
                }
            }

            expression.DataType = expressionFinalType;
        }

        private void CheckFunctionCall(AstItem expression)
        {
            var function = _context.GetFunction(expression.Identifier);
            if (function == null)
                throw new Exception("Undeclared function: " + expression.Identifier);

            if (!function.IsVariadic)
                Assert.Count(expression.Children.Count, function.Parameters.Count, "Invalid number of arguments to function non-variadic '" + function.Name + "'!");

            for (int i = 0; i < expression.Children.Count; i++)
            {
                var paramExpression = expression.Children[i];
                var dataType = CheckExpression(paramExpression);

                //TODO: Is this really required? When calling a function, the current stack frame still exists.
                if (dataType.Kind == DataTypeKind.ARRAY && dataType.MemoryRegion == MemoryRegion.Stack)
                    throw new Exception("Arrays on stack cannot be passed as parameter to function calls! Use heap array instead. In: " + expression + "; parameter: " + paramExpression);

                if (!function.IsVariadic || (function.IsVariadic && i < function.Parameters.Count))
                {
                    var parameter = function.Parameters[i];
                    if (dataType != parameter.DataType)
                        throw new Exception("Invalid data type for parameter " + parameter + "! Expected: " + parameter.DataType + ", found: " + dataType);
                }

                paramExpression.DataType = dataType;
            }

            expression.DataType = function.ReturnType;
        }

        private void CheckVariable(AstItem expression)
        {
            var variable = _context.GetSymbol(expression.Identifier);
            if (variable == null)
                throw new Exception("Undeclared variable: " + expression.Identifier);

            expression.DataType = variable.DataType;
        }

        private void CheckVector(AstItem expression)
        {
            if (expression.Identifier == "vec")
            {
                //Vector construction with type-inference
                DataType subType = null;
                foreach (var child in expression.Children)
                {
                    var childType = CheckExpression(child);

                    if (subType == null)
                    {
                        subType = childType;
                        if (subType.IsVector)
                            throw new Exception("Vectors of vectors are not allowed!");
                    }
                    else
                    {
                        if (subType != childType)
                            throw new Exception("All items in a vector have to have the same type! Expected: " + subType + ", found: " + childType);
                    }
                }

                var vectorSize = expression.Children.Count;
                var vectorType = DataType.GetVectorType(subType, vectorSize);

                if (vectorType == DataType.VOID)
                    throw new Exception("Vectors of " + subType + " cannot have length " + vectorSize);

                expression.DataType = vectorType;
            }
            else
            {
                //Vector construction with specific vector type
                var vectorType = DataType.FindByName(expression.Identifier);
                if (vectorType == null)
                    throw new Exception("Unknown vector type: " + expression.Identifier);

                if (!vectorType.IsVector)
                    throw new Exception("Expected vector type, got: " + vectorType);

                if (vectorType.NumElements != expression.Children.Count)
                    throw new Exception("Vector type " + vectorType.Name + " requires " + vectorType.NumElements + " elements, got " + expression.Children.Count);

                foreach (var child in expression.Children)
                {
                    var childType = CheckExpression(child);
                    if (childType != vectorType.ElementType)
                        throw new Exception("Invalid data type in vector constructor for " + vectorType.Name + "! Expected: " + vectorType.ElementType + ", found: " + childType);
                }

                expression.DataType = vectorType;
            }
        }

        private void CheckImmediate(AstItem expression)
        {
            var valueStr = (string)expression.Value;

            var dataType = expression.DataType;
            if (dataType == null)
                dataType = FindImmediateType(valueStr);

            var value = ParseImmediate(valueStr, dataType);

            expression.Value = value;
            expression.DataType = dataType;
        }

        private DataType FindImmediateType(string value)
        {
            if (value == "true" || value == "false")
                return DataType.BOOL;

            return FindNumberDataType(value);
        }

        private DataType FindNumberDataType(string value)
        {
            var last = value[value.Length - 1];

            if (!value.Contains("."))
            {
                var secondLast = '\0';
                if (value.Length >= 2)
                    secondLast = value[value.Length - 2];

                if (secondLast == 'u')
                {
                    switch (last)
                    {
                        case 'b':
                            return DataType.U8;
                        case 'w':
                            return DataType.U16;
                        case 'd':
                            return DataType.U32;
                        case 'q':
                            return DataType.U64;
                    }
                }
                else
                {
                    switch (last)
                    {
                        case 'b':
                            return DataType.I8;
                        case 'w':
                            return DataType.I16;
                        case 'd':
                            return DataType.I32;
                        case 'q':
                            return DataType.I64;
                        case 'u':
                            return DataType.U64;
                    }
                }

                return DataType.I64;
            }
            
            if (last == 'f')
                return DataType.F32;

            return DataType.F64;
        }

        private object ParseImmediate(string str, DataType dataType)
        {
            if (dataType.Kind == DataTypeKind.BOOL)
            {
                if (str == "true")
                    return true;
                else if (str == "false")
                    return false;
                else
                    throw new Exception("Unknown boolean value: " + str);
            }
            else if (dataType.Kind == DataTypeKind.STRING8)
            {
                return str;
            }
            else
                return ParseNumber(str, dataType);
        }

        private object ParseNumber(string str, DataType dataType)
        {
            var cleanNumberStr = str;
            while (_numberSuffixChars.Contains(cleanNumberStr[cleanNumberStr.Length - 1]))
                cleanNumberStr = cleanNumberStr.Substring(0, cleanNumberStr.Length - 1);

            switch (dataType.Kind)
            {
                case DataTypeKind.I8:
                    return sbyte.Parse(cleanNumberStr);

                case DataTypeKind.I16:
                    return short.Parse(cleanNumberStr);

                case DataTypeKind.I32:
                    return int.Parse(cleanNumberStr);

                case DataTypeKind.I64:
                    return long.Parse(cleanNumberStr);

                case DataTypeKind.U8:
                    return byte.Parse(cleanNumberStr);

                case DataTypeKind.U16:
                    return ushort.Parse(cleanNumberStr);

                case DataTypeKind.U32:
                    return uint.Parse(cleanNumberStr);

                case DataTypeKind.U64:
                    //"long" is not correct, but using "ulong" causes all kinds of issues with array size calculations, where a lot of signed values are used.
                    return long.Parse(cleanNumberStr);

                case DataTypeKind.F32:
                     return float.Parse(cleanNumberStr, CultureInfo.InvariantCulture);

                case DataTypeKind.F64:
                     return double.Parse(cleanNumberStr, CultureInfo.InvariantCulture);

                default:
                    throw new Exception("Unsupported number type: " + dataType + " for value " + str);
            }
        }

    }

}
