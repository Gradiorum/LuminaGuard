using System;

namespace LuminaGuard.Models
{
    public class CircadianAdvisor
    {
        public TimeSpan DesiredBedtime { get; set; }
        public TimeSpan DesiredWakeTime { get; set; }

        public CircadianAdvisor(TimeSpan bedtime, TimeSpan wakeTime)
        {
            DesiredBedtime = bedtime;
            DesiredWakeTime = wakeTime;
        }

        public double GetRecommendedColorTemperature(TimeSpan now)
        {
            double hoursToBed = (DesiredBedtime - now).TotalHours;
            if (hoursToBed < 0) hoursToBed += 24;
            if (hoursToBed < 2) return 2000; 
            if (hoursToBed < 4) return 3000;
            return 5000;
        }
    }
}
