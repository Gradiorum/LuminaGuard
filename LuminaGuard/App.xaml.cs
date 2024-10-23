// App.xaml.cs
using System.Windows;

namespace LuminaGuard
{
    public partial class App : Application
    {
        private static readonly string AppMutexName = "LuminaGuardAppMutex";
        private System.Threading.Mutex? appMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure only one instance runs
            bool createdNew;
            appMutex = new System.Threading.Mutex(true, AppMutexName, out createdNew);

            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("LuminaGuard is already running.", "LuminaGuard", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Prompt the user to disable Windows Night Light
            MessageBox.Show("For optimal performance, please disable Windows Night Light.", "LuminaGuard", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            appMutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
