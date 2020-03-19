using System;


namespace erc
{
    public static class Assert
    {
        public static void Check(bool check, string message)
        {
            if (!check)
                throw new Exception(message);
        }
    }
}
