namespace OJTManagementSystem.Helpers
{
    public static class DateTimeHelper
    {
        public static int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static List<DateTime> GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            var workingDays = new List<DateTime>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }

        public static int GetTotalWorkingDays(DateTime startDate, DateTime endDate)
        {
            return GetWorkingDays(startDate, endDate).Count;
        }

        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        public static int GetDaysDifference(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate.Date - startDate.Date).TotalDays;
        }

        public static bool IsWithinRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck.Date >= startDate.Date && dateToCheck.Date <= endDate.Date;
        }

        public static string GetFormattedTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        public static string GetFormattedDate(DateTime date)
        {
            return date.ToString("MMMM dd, yyyy");
        }

        public static string GetFormattedDateTime(DateTime dateTime)
        {
            return dateTime.ToString("MMMM dd, yyyy HH:mm");
        }
    }
}