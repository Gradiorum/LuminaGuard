using System;

namespace LuminaGuard.Helpers
{
    public class ScheduleEntry
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IntensityCurve { get; set; } = "Linear";

        public override string ToString()
        {
            return $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm} ({IntensityCurve})";
        }
    }
}
