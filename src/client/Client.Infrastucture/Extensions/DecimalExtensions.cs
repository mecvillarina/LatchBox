using System;
using System.Globalization;
using System.Numerics;

namespace Client.Infrastructure.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal ToAmount(this BigInteger amount, int decimals)
        {
            return Convert.ToDecimal(((double)amount) / Math.Pow(10, decimals));
        }

        public static decimal ToAmount(this BigInteger amount, byte decimals)
        {
            return Convert.ToDecimal(((double)amount) / Math.Pow(10, decimals));
        }

        public static BigInteger ToAmount(this decimal amount, int decimals)
        {
            return (BigInteger)((double)amount * Math.Pow(10, decimals));
        }

        public static BigInteger ToAmount(this double amount, int decimals)
        {
            return (BigInteger)(amount * Math.Pow(10, decimals));
        }

        public static int CountDecimalPlaces(this double dec)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = ",";

            string[] digits = dec.ToString().Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return digits.Length == 2 ? digits[1].Length : 0;
        }
    }
}
