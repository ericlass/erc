using System;
using System.Collections.Generic;

namespace erc
{
    public class PostProcessor
    {
        public void Process(CompilerContext context)
        {
            foreach (var function in context.AST.Children)
            {
                if (function.Kind == AstItemKind.FunctionDecl)
                    CreateVariableScopeNodes(function);
            }
            
            AssignDataNames(context.AST);
            //RemoveUnusedFunctionDecls(context.AST);
        }

        private int _immCounter = 0;

        private void AssignDataNames(AstItem item)
        {
            if (item.Kind == AstItemKind.Immediate || item.Kind == AstItemKind.Vector)
            {
                //Booleans get fixed names as they can only have two values
                if (item.DataType == DataType.BOOL)
                {
                    var boolVal = (bool)item.Value;
                    item.Identifier = boolVal ? "imm_bool_true" : "imm_bool_false";
                }
                else
                {
                    item.Identifier = "imm_" + _immCounter;
                    _immCounter += 1;
                }
            }

            foreach (var child in item.Children)
            {
                if (child != null)
                    AssignDataNames(child);
            }
        }

        /// <summary>
        /// Inject marker statements in the list of statement after the last usage of each variable
        /// </summary>
        /// <param name="scope">The scope to check.</param>
        public void CreateVariableScopeNodes(AstItem scope)
        {
            var statements = scope.Children[1].Children;
            var varNames = new List<string>();
            var alreadyFound = new HashSet<string>();
            for (int i = statements.Count - 1; i >= 0; i--)
            {
                var statement = statements[i];
                //TODO: Does not work if variable is declared in parent scope
                /*if (CreatesNewScope(statement))
                    CreateVariableScopeNodes(statement);*/

                FindVariables(statement, varNames);
                foreach (var varName in varNames)
                {
                    if (!alreadyFound.Contains(varName))
                    {
                        statements.Insert(i + 1, AstItem.VarScopeEnd(varName));
                        alreadyFound.Add(varName);
                    }
                }
                varNames.Clear();
            }
        }

        /// <summary>
        /// Determines if the given AST item creates a new scope or not.
        /// </summary>
        /// <param name="item">The item to check.</param>
        private bool CreatesNewScope(AstItem item)
        {
            return item.Kind == AstItemKind.If;
        }


        /// <summary>
        /// Recursily find all mentions of variables in the given AST item and it's children.
        /// </summary>
        /// <param name="item">The item to search.</param>
        /// <param name="found">Found variable names are stored here.</param>
        public void FindVariables(AstItem item, List<string> found)
        {
            if (item.Kind == AstItemKind.Variable || item.Kind == AstItemKind.VarDecl || item.Kind == AstItemKind.Assignment)
            {
                found.Add(item.Identifier);
            }

            foreach (var child in item.Children)
            {
                if (child != null)
                    FindVariables(child, found);
            }
        }

        public void RemoveUnusedFunctionDecls(AstItem topItem)
        {
            //TODO: This does not work for functions that are called in functions that are not called. Make it work.
            var calledFunctions = new List<string>();
            FindFunctionCalls(topItem, calledFunctions);

            for (int i = topItem.Children.Count - 1; i >= 0; i--)
            {
                var func = topItem.Children[i];
                if (func.Kind == AstItemKind.FunctionDecl && !calledFunctions.Contains(func.Identifier))
                {
                    topItem.Children.RemoveAt(i);
                }
            }
        }

        public void FindFunctionCalls(AstItem topItem, List<string> result)
        {
            if (topItem.Kind == AstItemKind.FunctionCall)
            {
                result.Add(topItem.Identifier);
            }

            foreach (var child in topItem.Children)
            {
                FindFunctionCalls(child, result);
            }
        }

    }
}
