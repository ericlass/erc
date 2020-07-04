using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public static class Operator
    {
        private static List<IBinaryOperator> _allValues = null;

        public static List<IBinaryOperator> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(Operator);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<IBinaryOperator>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as IBinaryOperator);
                }
            }

            return _allValues;
        }

        public static IBinaryOperator Parse(string str)
        {
            var allValues = GetAllValues();
            return allValues.Find((o) => o.Figure == str);
        }

        //############################################################################

        public static IBinaryOperator MUL = new MultiplicationOperator();
        public static IBinaryOperator DIV = new DivisionOperator();
        public static IBinaryOperator ADD = new AdditionOperator();
        public static IBinaryOperator SUB = new SubtractionOperator();

        public static IBinaryOperator LessThan = new RelationalOperator("<", IMInstruction.SETL);
        public static IBinaryOperator LessThanOrEqual = new RelationalOperator("<=", IMInstruction.SETLE);
        public static IBinaryOperator GreaterThan = new RelationalOperator(">", IMInstruction.SETG);
        public static IBinaryOperator GreaterThanOrEqual = new RelationalOperator(">=", IMInstruction.SETGE);

        public static IBinaryOperator EQUALS = new EqualityOperator("==", false);
        public static IBinaryOperator NOT_EQUALS = new EqualityOperator("!=", true);

        public static IBinaryOperator AND_BOOL = new BooleanOperator("&&", IMInstruction.AND, 12);
        public static IBinaryOperator OR_BOOL = new BooleanOperator("||", IMInstruction.OR, 11);

        //Only required for expression parsing, no code generated
        public static IBinaryOperator ROUND_BRACKET_OPEN = new NoOpOperator("(", 1);
        //Only required for expression parsing, no code generated
        public static IBinaryOperator ROUND_BRACKET_CLOSE = new NoOpOperator(")", 1);
    }

}
