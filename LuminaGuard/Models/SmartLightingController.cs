namespace LuminaGuard.Models
{
    public interface SmartLightingController
    {
        void Initialize();
        void SetColorTemperature(double kelvin);
    }
}
