// App.xaml.cs
using System.Windows;

namespace LuminaGuard
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Prompt the user to disable Windows Night Light
            MessageBox.Show("For optimal performance, please disable Windows Night Light.", "LuminaGuard", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
