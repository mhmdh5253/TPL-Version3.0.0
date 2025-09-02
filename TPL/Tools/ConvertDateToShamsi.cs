using System.Globalization;

namespace TPLWeb.Tools
{
    public class ConvertDateToShamsi
    {
        public string ConvertToShamsi(string date)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            string[] miladiDateFormats = {
                "MM/dd/yyyy", "M/d/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy/MM/dd", "yyyy-MM-dd",
                "yyyyMMdd", "dd/MM/yyyy", "d/M/yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/M/yy",
                "MM/dd/yy", "M/d/yy", "dd MMM yyyy", "d MMM yyyy", "MMM d, yyyy",
                "MMMM d, yyyy", "MM-dd-yyyy", "M-d-yyyy", "dd.MM.yyyy", "d.M.yyyy",
                "yyyy.MM.dd", "yyyy.MM.d", "d MMMM yyyy", "dd MMMM yyyy", "yyyyMMddTHHmmssZ"
            };
            DateTime dateValue;
            bool isMiladiParsed = DateTime.TryParseExact(date, miladiDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);

            if (isMiladiParsed && dateValue.Year >= 1900 && dateValue.Year <= 2100)
            {
                int year = persianCalendar.GetYear(dateValue);
                int month = persianCalendar.GetMonth(dateValue);
                int day = persianCalendar.GetDayOfMonth(dateValue);

                date = $"{year}/{month}/{day}";
            }
            else
            {
                string originalDate = date!;
                string[] dateParts = originalDate.Split('/');
                date = $"{dateParts[2]}/{dateParts[1]}/{dateParts[0]}";

            }
            return date;
        }
    }
}
