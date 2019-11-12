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
                CreateVariableScopeNodes(function);
            }
            
            AssignDataNames(context.AST);
        }

        private int _immCounter = 0;

        private void AssignDataNames(AstItem item)
        {
            if (item.Kind == AstItemKind.Immediate || item.Kind == AstItemKind.Vector)
            {
                item.Identifier = "imm_" + _immCounter;
                _immCounter += 1;
            }

            foreach (var child in item.Children)
            {
                AssignDataNames(child);
            }
        }

        /// <summary>
        /// Inject marker statements in the list of statement after the last usage of each variable
        /// </summary>
        /// <param name="scope">The scope to check.</param>
        public void CreateVariableScopeNodes(AstItem scope)
        {
            var statements = scope.Children;
            var varNames = new List<string>();
            var alreadyFound = new HashSet<string>();
            for (int i = statements.Count - 1; i >= 0; i--)
            {
                FindVariables(statements[i], varNames);
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
                FindVariables(child, found);
            }
        }

    }
}
