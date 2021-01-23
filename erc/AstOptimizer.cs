using System;
using System.Collections.Generic;

namespace erc
{
    public class AstOptimizer
    {
        public void Optimize(AstItem programItem)
        {
            Assert.AstItemKind(programItem.Kind, AstItemKind.Programm, "Invalid AST item for optimization");
            EvaluateConstantExpressions(programItem);
        }

        private void EvaluateConstantExpressions(AstItem item)
        {
            if (item.Kind == AstItemKind.Expression)
            {
                for (int i = 0; i < item.Children.Count; i++)
                {
                    var child = item.Children[i];
                    if (child.Kind == AstItemKind.BinaryOperator && child.BinaryOperator.Figure == "::")
                    {
                        //Convert enum value access like "Enum::Value" to immediate values
                        var typeItem = item.Children[i - 2];
                        Assert.AstItemKind(typeItem.Kind, AstItemKind.Type, "Invalid kind of first operand in expression");
                        var valueItem = item.Children[i - 1];
                        Assert.AstItemKind(valueItem.Kind, AstItemKind.Identifier, "Invalid kind of second operand in expression");

                        var element = typeItem.DataType.EnumElements.Find((e) => e.Name == valueItem.Identifier);
                        Assert.True(element != null, "Enum element with name not found: " + valueItem.Identifier + " in enum: " + typeItem.Identifier);

                        child.Kind = AstItemKind.Immediate;
                        child.DataType = DataType.U32;
                        child.Value = element.Index;
                        child.Operator = null;

                        item.Children.RemoveAt(i - 2);
                        item.Children.RemoveAt(i - 2);

                        i -= 2;
                    }
                }
            }
            else
            {
                foreach (var child in item.Children)
                {
                    if (child != null)
                        EvaluateConstantExpressions(child);
                }
            }
        }
    }

}
