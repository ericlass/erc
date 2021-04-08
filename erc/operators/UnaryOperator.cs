using System;
using System.Collections.Generic;
using System.Reflection;

namespace erc
{
    public static class UnaryOperator
    {
        private static List<IUnaryOperator> _allValues = null;

        public static List<IUnaryOperator> GetAllValues()
        {
            if (_allValues == null)
            {
                var regType = typeof(UnaryOperator);
                var fields = regType.GetFields(BindingFlags.Public | BindingFlags.Static);
                _allValues = new List<IUnaryOperator>(fields.Length);
                foreach (var field in fields)
                {
                    _allValues.Add(field.GetValue(null) as IUnaryOperator);
                }
            }

            return _allValues;
        }

        public static IUnaryOperator Parse(string str)
        {
            var allValues = GetAllValues();
            return allValues.Find((o) => o.Figure == str);
        }

        //############################################################################

        public static IUnaryOperator NEGATION = new ArithmeticNegationOperator();
        public static IUnaryOperator BOOLEAN_INVERT = new BooleanInversionOperator();
        public static IUnaryOperator POINTER_DEREFERENCE = new PointerDereferenceOperator();
        public static IUnaryOperator ADDRESS_OF = new AddressOperator();
        public static IUnaryOperator ARRAY_LENGTH = new ArrayLengthOperator();
    }
}
