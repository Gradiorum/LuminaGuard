using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LuminaGuard.Helpers
{
    public class FilterManager
    {
        private List<Func<Color, Color>> filters;
        private string accessibilityMode = "None";

        public FilterManager()
        {
            filters = new List<Func<Color, Color>>();
        }

        public void SetAccessibilityMode(string mode)
        {
            accessibilityMode = mode ?? "None";
        }

        public Color ApplyFilters(Color color)
        {
            filters.Clear();

            switch (accessibilityMode)
            {
                case "High Contrast":
                    filters.Add(HighContrastFilter);
                    break;
                case "Darkroom":
                    filters.Add(DarkroomFilter);
                    break;
                case "Deuteranope Simulation":
                    filters.Add(DeuteranopeFilter);
                    break;
                case "Protanope Simulation":
                    filters.Add(ProtanopeFilter);
                    break;
                case "Tritanope Simulation":
                    filters.Add(TritanopeFilter);
                    break;
                case "None":
                default:
                    break;
            }

            foreach (var filter in filters)
            {
                color = filter(color);
            }
            return color;
        }

        public static Color HighContrastFilter(Color color)
        {
            byte gray = (byte)((color.R + color.G + color.B) / 3);
            return gray < 128 ? Colors.Black : Colors.White;
        }

        public static Color DarkroomFilter(Color color)
        {
            return Color.FromArgb(color.A, (byte)(255 - color.R), 0, 0);
        }

        // Naive color blindness simulations
        public static Color DeuteranopeFilter(Color color)
        {
            var avg = (color.R + color.B) / 2;
            return Color.FromArgb(color.A, (byte)avg, (byte)avg, color.B);
        }

        public static Color ProtanopeFilter(Color color)
        {
            var avg = (color.G + color.B) / 2;
            return Color.FromArgb(color.A, (byte)avg, color.G, (byte)avg);
        }

        public static Color TritanopeFilter(Color color)
        {
            var avg = (color.R + color.G) / 2;
            return Color.FromArgb(color.A, color.R, (byte)avg, (byte)avg);
        }
    }
}
