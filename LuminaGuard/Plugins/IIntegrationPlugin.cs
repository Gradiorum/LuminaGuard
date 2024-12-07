namespace LuminaGuard.Plugins
{
    public interface IIntegrationPlugin
    {
        string Name { get; }
        void Initialize();
        void OnSettingsChanged();
    }
}
