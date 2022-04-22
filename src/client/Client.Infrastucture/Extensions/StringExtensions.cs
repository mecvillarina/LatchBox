using Neo;
using Neo.VM.Types;
using System.Numerics;

namespace Client.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToAmountDisplay(this decimal amount, int decimals)
        {
            return amount.ToString($"0.{new string('#', decimals)}");
        }

        public static string ToMask(this string value, int length)
        {
            if (value == null) return "";

            return string.Format("{0}....{1}", value.Substring(0, length), value.Substring(value.Length - (length), length));
        }
    }
}
