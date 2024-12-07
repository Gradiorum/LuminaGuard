using System.Windows;
using System.Threading;
using LuminaGuard.Models;
using System.IO;
using System.Text.Json;
using System;

namespace LuminaGuard
{
    public partial class App : Application
    {
        private static readonly string AppMutexName = "LuminaGuardAppMutex";
        private Mutex? appMutex;
        public static UserConfig CurrentConfig { get; set; } = new UserConfig();
        public static ProfilesManager ProfilesManager { get; set; } = new ProfilesManager();

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            appMutex = new Mutex(true, AppMutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("LuminaGuard is already running.", "LuminaGuard", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            LoadConfiguration();

            // Conditional onboarding: If first run not completed, show onboarding. Otherwise, show MainWindow.
            if (CurrentConfig.FirstRunCompleted)
            {
                // Override StartupUri logic: we already have OnboardingWizard as default, but we will skip it:
                StartupUri = null;
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            // Else: OnboardingWizard is shown as defined in App.xaml
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SaveConfiguration();
            appMutex?.ReleaseMutex();
            base.OnExit(e);
        }

        private void LoadConfiguration()
        {
            string configPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                try
                {
                    var config = JsonSerializer.Deserialize<UserConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (config != null)
                    {
                        // If versioning is introduced, handle migrations here:
                        // e.g., if (config.ConfigVersion < CurrentConfig.LatestSupportedVersion) { Migrate(config); }
                        CurrentConfig = config;
                    }
                }
                catch (Exception ex)
                {
                    Helpers.Logging.Log("Error loading config: " + ex.ToString());
                    // Fallback to defaults if broken
                    CurrentConfig = new UserConfig();
                }
            }
            else
            {
                CurrentConfig = new UserConfig();
            }

            ProfilesManager.LoadProfiles();
        }

        private void SaveConfiguration()
        {
            string configPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            try
            {
                string json = JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
                ProfilesManager.SaveProfiles();
            }
            catch (Exception ex)
            {
                Helpers.Logging.Log("Error saving config: " + ex.ToString());
            }
        }
    }
}
