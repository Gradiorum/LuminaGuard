using System.ComponentModel;
using System.Runtime.CompilerServices;
using LuminaGuard.Models;

namespace LuminaGuard.ViewModels
{
    // Minimal example of a ViewModel for future MVVM:
    // Bind UI controls to properties here instead of code-behind.
    // Implement property change notifications and apply updates to App.CurrentConfig.
    // The actual binding in MainWindow.xaml would need adjustments (DataContext etc.).

    public class MainViewModel : INotifyPropertyChanged
    {
        private double intensity;
        private bool filterEnabled;

        public double Intensity
        {
            get => intensity;
            set
            {
                if (intensity != value)
                {
                    intensity = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool FilterEnabled
        {
            get => filterEnabled;
            set
            {
                if (filterEnabled != value)
                {
                    filterEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            // Initialize from App.CurrentConfig if needed
            if (App.CurrentConfig != null)
            {
                this.intensity = App.CurrentConfig.Intensity;
                this.filterEnabled = App.CurrentConfig.FilterEnabled;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
