using System;
using System.Windows;

namespace LuminaGuard.Views
{
    public partial class OnboardingWizard : Window
    {
        public OnboardingWizard()
        {
            InitializeComponent();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSpan.TryParse(WakeTimeTextBox.Text, out TimeSpan wake) &&
                TimeSpan.TryParse(BedTimeTextBox.Text, out TimeSpan bed))
            {
                App.CurrentConfig.DesiredWakeTime = wake;
                App.CurrentConfig.DesiredBedtime = bed;
            }

            App.CurrentConfig.FirstRunCompleted = true;
            // Save config now in case app crashes before main window:
            ((App)Application.Current).SaveConfiguration();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
