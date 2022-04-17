namespace Client.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToBalanceDisplay(this decimal balance, int decimals)
        {
            return balance.ToString($"0.{new string('#', decimals)}");
        }
    }
}
