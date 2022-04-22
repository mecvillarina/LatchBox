using System;
using System.Globalization;
using System.Numerics;

namespace Client.Infrastructure.Extensions
{
    public static class NumberExtensions
    {
        public static decimal ToAmount(this BigInteger value, int decimals)
        {
            return Convert.ToDecimal(((double)value) / Math.Pow(10, decimals));
        }

        public static decimal ToAmount(this BigInteger value, byte decimals)
        {
            return Convert.ToDecimal(((double)value) / Math.Pow(10, decimals));
        }

        public static BigInteger ToAmount(this decimal value, int decimals)
        {
            return (BigInteger)((double)value * Math.Pow(10, decimals));
        }

        public static BigInteger ToAmount(this double value, int decimals)
        {
            return (BigInteger)(value * Math.Pow(10, decimals));
        }

        public static int CountDecimalPlaces(this double value)
        {
            string[] digits = value.ToString().Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return digits.Length == 2 ? digits[1].Length : 0;
        }
    }
}
