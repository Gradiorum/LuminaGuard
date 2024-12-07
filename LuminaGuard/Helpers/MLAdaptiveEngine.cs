using LuminaGuard.Models;
using System;

namespace LuminaGuard.Helpers
{
    public static class MLAdaptiveEngine
    {
        public static void AdaptSettings(UserConfig config)
        {
            // Placeholder logic. Future versions could incorporate real ML (e.g., user feedback loops, data from sensors).
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now.Hours >= 22 || now.Hours < 6)
            {
                config.ColorTemperature = Math.Max(1000, config.ColorTemperature - 10);
            }
            else
            {
                config.ColorTemperature = Math.Min(6500, config.ColorTemperature + 10);
            }
        }
    }
}
