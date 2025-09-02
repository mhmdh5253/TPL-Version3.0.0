namespace DAL
{
    public static class ConvertToEngilshNumbers
    {
        public static string ConvertToEngilshNumber(string? input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Replace("۰", "0").Replace("۱", "1").Replace("۲", "2").Replace("۳", "3")
                    .Replace("۴", "4").Replace("۵", "5").Replace("۶", "6").Replace("۷", "7").Replace("۸", "8")
                    .Replace("۹", "9");

                return input;
            }

            return string.Empty;
        }
    }
}
