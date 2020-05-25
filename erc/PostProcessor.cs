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
            if (item.Kind == AstItemKind.Variable || item.Kind == AstItemKind.VarDecl || item.Kind == AstItemKind.DelPointer || item.Kind == AstItemKind.IndexAccess || item.Kind == AstItemKind.PointerDeref)
            {
                found.Add(item.Identifier);
            }

            foreach (var child in item.Children)
            {
                if (child != null)
                    FindVariables(child, found);
            }
        }

    }
}
