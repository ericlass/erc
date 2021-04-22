using System;

namespace erc
{
    public static class StringUtils
    {
        public static string CharToPrintableStr(char c)
        {
            if (c < 32 || (c > 126 && c < 160))
            {
                var num = (ushort)c;
                return "\\" + num.ToString();
            }

            return c.ToString();
        }
    }
}
