// FilterManager.cs
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LuminaGuard.Helpers
{
    public class FilterManager
    {
        private List<Func<Color, Color>> filters;

        public FilterManager()
        {
            filters = new List<Func<Color, Color>>();
            // Initialize with default filters if any
        }

        public void AddFilter(Func<Color, Color> filter)
        {
            filters.Add(filter);
        }

        public void RemoveFilter(Func<Color, Color> filter)
        {
            filters.Remove(filter);
        }

        public Color ApplyFilters(Color color)
        {
            foreach (var filter in filters)
            {
                color = filter(color);
            }
            return color;
        }

        // Example filter: Grayscale
        public void InitializeDefaultFilters()
        {
            AddFilter(GrayscaleFilter);
            // Add other default filters here
        }

        public static Color GrayscaleFilter(Color color)
        {
            byte gray = (byte)((color.R + color.G + color.B) / 3);
            return Color.FromArgb(color.A, gray, gray, gray);
        }
    }
}
