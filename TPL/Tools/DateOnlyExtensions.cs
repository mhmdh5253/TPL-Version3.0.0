using System.Globalization;

namespace TPLWeb.Tools
{
    public static class DateOnlyExtensions
    {
        public static string ToPersianString(this DateOnly date, bool isNow = false)
        {
            if (isNow)
            {
                date = DateOnly.FromDateTime(DateTime.Now);
            }

            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(date.ToDateTime(new TimeOnly(0, 0)));
            var month = persianCalendar.GetMonth(date.ToDateTime(new TimeOnly(0, 0)));
            var day = persianCalendar.GetDayOfMonth(date.ToDateTime(new TimeOnly(0, 0)));

            return $"{year:0000}-{month:00}-{day:00}";
        }
    }

}
