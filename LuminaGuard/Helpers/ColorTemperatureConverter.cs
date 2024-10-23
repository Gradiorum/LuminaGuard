// ColorTemperatureConverter.cs
using System;
using System.Windows.Media;

namespace LuminaGuard.Helpers
{
    public static class ColorTemperatureConverter
    {
        public static Color ColorTemperatureToRGB(double kelvin)
        {
            kelvin = kelvin / 100;

            double red, green, blue;

            // Calculate red
            if (kelvin <= 66)
            {
                red = 255;
            }
            else
            {
                red = kelvin - 60;
                red = 329.698727446 * Math.Pow(red, -0.1332047592);
                red = Math.Clamp(red, 0, 255);
            }

            // Calculate green
            if (kelvin <= 66)
            {
                green = kelvin;
                green = 99.4708025861 * Math.Log(green) - 161.1195681661;
            }
            else
            {
                green = kelvin - 60;
                green = 288.1221695283 * Math.Pow(green, -0.0755148492);
            }
            green = Math.Clamp(green, 0, 255);

            // Calculate blue
            if (kelvin >= 66)
            {
                blue = 255;
            }
            else if (kelvin <= 19)
            {
                blue = 0;
            }
            else
            {
                blue = kelvin - 10;
                blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;
                blue = Math.Clamp(blue, 0, 255);
            }

            return Color.FromRgb((byte)red, (byte)green, (byte)blue);
        }
    }
}
