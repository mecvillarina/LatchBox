using System;
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
            string[] digits = dec.ToString().Split('.');
            return digits.Length == 2 ? digits[1].Length : 0;
        }
    }
}
