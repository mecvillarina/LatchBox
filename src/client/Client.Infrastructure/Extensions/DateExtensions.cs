using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Extensions
{
    public static class DateExtensions
    {
        public static DateTime ToCurrentTimeZone(this DateTime date)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return date.Add(offset);
        }

        public static DateTime? ToCurrentTimeZone(this DateTime? date)
        {
            if (!date.HasValue) return null;

            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            return date.Value.Add(offset);
        }

        public static string ToFormat(this DateTime date, string format)
        {
            return date.ToString(format);
        }

        public static string ToFormat(this DateTime? date, string format)
        {
            if (!date.HasValue)
                return "-";

            return ToFormat(date.Value, format);
        }
    }
}
