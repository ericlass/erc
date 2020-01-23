using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public static class Operator
    {
        private static List<IOperator> _allValues = null;

        public static List<IOperator> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(Operator);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<IOperator>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as IOperator);
                }
            }

            return _allValues;
        }

        public static IOperator Parse(string str)
        {
            var allValues = GetAllValues();
            return allValues.Find((o) => o.Figure == str);
        }

        //############################################################################

        public static IOperator MUL = new MultiplicationOperator();
        public static IOperator DIV = new DivisionOperator();
        public static IOperator ADD = new AdditionOperator();
        public static IOperator SUB = new SubtractionOperator();

        public static IOperator EQUALS = new EqualityOperator("==", false);
        public static IOperator NOT_EQUALS = new EqualityOperator("!=", true);

        public static IOperator AND_BOOL = new BooleanOperator("&&", Instruction.AND, 12);
        public static IOperator OR_BOOL = new BooleanOperator("||", Instruction.OR, 11);

        //Only required for expression parsing, no code generated
        public static IOperator ROUND_BRACKET_OPEN = new NoOpOperator("(", 1);
        //Only required for expression parsing, no code generated
        public static IOperator ROUND_BRACKET_CLOSE = new NoOpOperator(")", 1);
    }

}
