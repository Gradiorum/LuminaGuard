using System;
using System.Collections.Generic;

namespace LuminaGuard.Helpers
{
    public class Scheduler
    {
        public IList<ScheduleEntry> ScheduleEntries { get; set; }
        public double RateOfChange { get; set; } = 1.0;

        public Scheduler()
        {
            ScheduleEntries = new List<ScheduleEntry>();
        }

        public double CalculateProgress(TimeSpan now)
        {
            foreach (var entry in ScheduleEntries)
            {
                TimeSpan start = entry.StartTime;
                TimeSpan end = entry.EndTime;

                if (end <= start)
                {
                    // Wraps past midnight
                    if (now >= start || now <= end)
                    {
                        return CalculateProgressForEntry(now, start, end, entry.IntensityCurve) * RateOfChange;
                    }
                }
                else
                {
                    if (now >= start && now <= end)
                    {
                        return CalculateProgressForEntry(now, start, end, entry.IntensityCurve) * RateOfChange;
                    }
                }
            }

            return 0; 
        }

        private double CalculateProgressForEntry(TimeSpan now, TimeSpan start, TimeSpan end, string curveType)
        {
            double totalMinutes = (end <= start)
                ? (end + TimeSpan.FromDays(1) - start).TotalMinutes
                : (end - start).TotalMinutes;
            double elapsedMinutes = (end <= start && now < start)
                ? (now + TimeSpan.FromDays(1) - start).TotalMinutes
                : (now - start).TotalMinutes;

            if (totalMinutes <= 0)
                return 1;

            double progress = elapsedMinutes / totalMinutes;
            progress = Math.Max(0, Math.Min(1, progress));

            switch (curveType)
            {
                case "Exponential":
                    progress = Math.Pow(progress, 2);
                    break;
                case "Linear":
                default:
                    break;
            }

            return progress;
        }
    }
}
