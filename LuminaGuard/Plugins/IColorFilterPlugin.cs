using System.Windows.Media;

namespace LuminaGuard.Plugins
{
    public interface IColorFilterPlugin
    {
        string Name { get; }
        Color ApplyFilter(Color input);
    }
}
