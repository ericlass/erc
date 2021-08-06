using System;
using System.Collections.Generic;

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

        public static string Escape(string str)
        {
            var result = str.Replace("\n", "\\n");
            result = result.Replace("\r", "\\r");
            result = result.Replace("\0", "\\0");
            result = result.Replace("\t", "\\t");
            result = result.Replace("\b", "\\b");
            return result;
        }

    }
}
