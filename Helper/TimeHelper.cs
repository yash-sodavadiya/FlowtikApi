namespace Flowtik.Helper
{
    public static class TimeHelper
    {
        public static decimal GetTotalHours(DateTime? endTime, DateTime? startTime)
        {
            if (endTime.HasValue && startTime.HasValue)
            {
                TimeSpan duration = endTime.Value - startTime.Value;
                return (decimal)duration.TotalHours;
            }
            return 0;
        }

        public static string FormatHours(decimal hours)
        {
            int hrs = (int)hours;
            int mins = (int)((hours - hrs) * 60);
            return $"{hrs:D2}h {mins:D2}m";
        }

        public static bool IsEightHourCompleted(decimal workedHours)
        {
            return workedHours >= 8.0m;
        }

        public static decimal GetRemainingHours(decimal workedHours)
        {
            const decimal requiredHours = 8.0m;
            return requiredHours > workedHours ? requiredHours - workedHours : 0;
        }
    }
}
