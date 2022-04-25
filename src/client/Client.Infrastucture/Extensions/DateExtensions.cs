using System;
using System.Numerics;

namespace Client.Infrastructure.Extensions
{
    public static class DateExtensions
    {
        public static DateTimeOffset ToCurrentTimeZone(this DateTimeOffset date)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return date.Add(offset);
        }

        public static DateTimeOffset? ToCurrentTimeZone(this DateTimeOffset? date)
        {
            if (!date.HasValue) return null;

            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return date.Value.Add(offset);
        }

        public static string ToFormat(this DateTimeOffset? date, string format)
        {
            if (!date.HasValue)
                return "-";

            return ToFormat(date.Value, format);
        }

        public static DateTimeOffset ToDateTimeOffsetFromMilliseconds(this BigInteger value)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)value);
        }
    }
}
