using System;


namespace erc
{
    public static class Assert
    {
        public static void True(bool boolean, string message)
        {
            if (!boolean)
                throw new Exception(message);
        }

        public static void AstItemKind(AstItemKind actual, AstItemKind expected, string message)
        {
            if (actual != expected)
                throw new Exception(FormatMessage(actual, expected, message));
        }

        public static void DataTypeKind(DataTypeKind actual, DataTypeKind expected, string message)
        {
            if (actual != expected)
                throw new Exception(FormatMessage(actual, expected, message));
        }

        public static void DataTypeGroup(DataTypeGroup actual, DataTypeGroup expected, string message)
        {
            if (actual != expected)
                throw new Exception(FormatMessage(actual, expected, message));
        }

        public static void IMOperandKind(IMOperandKind actual, IMOperandKind expected, string message)
        {
            if (actual != expected)
                throw new Exception(FormatMessage(actual, expected, message));
        }

        public static void Count(int actual, int expected, string message)
        {
            if (actual != expected)
                throw new Exception(FormatMessage(actual, expected, message));
        }

        private static string FormatMessage(object actual, object expected, string message)
        {
            return message.Trim() + " [expected: '" + expected + "'; actual: '" + actual + "'";
        }
    }
}
