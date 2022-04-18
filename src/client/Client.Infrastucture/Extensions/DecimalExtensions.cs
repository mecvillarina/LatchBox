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
    }
}
