using System.Numerics;

namespace Client.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToAmountDisplay(this decimal amount, int decimals)
        {
            return amount.ToString($"0.{new string('#', decimals)}");
        }
    }
}
