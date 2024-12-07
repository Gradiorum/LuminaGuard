using System;
using System.Collections.Generic;
using LuminaGuard.Helpers;

namespace LuminaGuard.Models
{
    public class UserConfig
    {
        // Versioning for future migrations
        public int ConfigVersion { get; set; } = 1;

        public string CurrentProfileName { get; set; } = "Default";
        public double Intensity { get; set; } = 50.0;
        public double ColorTemperature { get; set; } = 1200.0;
        public byte Brightness { get; set; } = 100;
        public bool FilterEnabled { get; set; } = false;
        public string CustomColorHex { get; set; } = "";
        public bool SchedulerEnabled { get; set; } = false;
        public bool BrightnessOverTimeEnabled { get; set; } = false;
        public bool SmartLightingEnabled { get; set; } = false;
        public bool MLAdaptationEnabled { get; set; } = false;
        public string AccessibilityMode { get; set; } = "None";
        public List<ScheduleEntry> Schedules { get; set; } = new List<ScheduleEntry>();

        public TimeSpan DesiredBedtime { get; set; } = new TimeSpan(23, 0, 0);
        public TimeSpan DesiredWakeTime { get; set; } = new TimeSpan(7, 0, 0);
        public double SleepDurationGoal { get; set; } = 8.0; // hours
        public string Location { get; set; } = "Unknown";

        public bool FirstRunCompleted { get; set; } = false;
        public bool HideNightLightWarning { get; set; } = false;
    }
}
