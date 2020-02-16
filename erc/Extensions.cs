using System;
using System.Globalization;

namespace erc
{
    static class Extensions
    {
        public static string ToCode(this float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string ToCode(this double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}
