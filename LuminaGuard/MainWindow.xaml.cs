// MainWindow.xaml.cs
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using Xceed.Wpf.Toolkit;
using LuminaGuard.Helpers;
using System.Windows.Controls;
using System.Windows.Forms; // For ColorDialog

namespace LuminaGuard
{
    public partial class MainWindow : Window
    {
        private OverlayWindow overlayWindow;
        private DispatcherTimer schedulerTimer;
        private Scheduler scheduler;
        private BrightnessController brightnessController;
        private FilterManager filterManager;
        private ObservableCollection<ScheduleEntry> schedules;
        private bool isHotkeyRegistered = false;

        public MainWindow()
        {
            InitializeComponent();
            overlayWindow = new OverlayWindow();
            scheduler = new Scheduler();
            brightnessController = new BrightnessController();
            filterManager = new FilterManager();
            schedules = new ObservableCollection<ScheduleEntry>();
            ScheduleListBox.ItemsSource = schedules;

            InitializeScheduler();
            RegisterHotkeys();

            // Event handlers
            this.Closing += MainWindow_Closing;
            EnableSchedulerCheckBox.Checked += EnableSchedulerCheckBox_Checked;
            EnableSchedulerCheckBox.Unchecked += EnableSchedulerCheckBox_Unchecked;
        }

        private void EnableFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (EnableFilterCheckBox.IsChecked == true)
            {
                ApplyOverlaySettings();
                overlayWindow.Show();
            }
            else
            {
                overlayWindow.Hide();
            }
        }

        private void IntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyOverlaySettings();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte brightness = (byte)BrightnessSlider.Value;
            brightnessController.SetBrightness(brightness);
        }

        private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HexCodeTextBox.Text = ""; // Clear hex code to use temperature
            ApplyOverlaySettings();
        }

        private void HexCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyOverlaySettings();
        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = colorDialog.Color;
                HexCodeTextBox.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }

        private bool IsValidHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return false;
            hex = hex.Trim();
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            return hex.Length == 6 || hex.Length == 8;
        }

        private void ApplyOverlaySettings()
        {
            if (overlayWindow == null) return;

            Color color;

            if (IsValidHex(HexCodeTextBox.Text))
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(HexCodeTextBox.Text);
                }
                catch
                {
                    // Handle invalid hex code
                    System.Windows.MessageBox.Show("Invalid hex code entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                // Use color temperature
                double kelvin = TemperatureSlider.Value;
                color = ColorTemperatureConverter.ColorTemperatureToRGB(kelvin);
            }

            double intensity = IntensitySlider.Value / 100.0;
            byte alpha = (byte)(intensity * 255);

            // Prevent full opacity
            if (alpha >= 230) // Approx 90% opacity
            {
                alpha = 230;
            }

            color = Color.FromArgb(alpha, color.R, color.G, color.B);

            // Apply custom filters
            color = filterManager.ApplyFilters(color);

            overlayWindow.SetOverlayColor(color);
        }

        private void InitializeScheduler()
        {
            schedulerTimer = new DispatcherTimer();
            schedulerTimer.Interval = TimeSpan.FromMinutes(1);
            schedulerTimer.Tick += SchedulerTimer_Tick;
            schedulerTimer.Start();
        }

        private void SchedulerTimer_Tick(object? sender, EventArgs e)
        {
            if (EnableSchedulerCheckBox.IsChecked == true)
            {
                UpdateIntensityBasedOnSchedule();
            }
        }

        private void UpdateIntensityBasedOnSchedule()
        {
            DateTime now = DateTime.Now;

            double progress = scheduler.CalculateProgress(now.TimeOfDay);

            double scheduledIntensity = progress * IntensitySlider.Maximum;
            IntensitySlider.Value = scheduledIntensity;

            if (EnableBrightnessAdjustmentCheckBox.IsChecked == true)
            {
                byte brightness = (byte)(BrightnessSlider.Maximum - scheduledIntensity); // Decrease brightness over time
                brightnessController.SetBrightness(brightness);
            }
        }

        private void EnableSchedulerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            schedulerTimer.Start();
        }

        private void EnableSchedulerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            schedulerTimer.Stop();
        }

        private void RegisterHotkeys()
        {
            if (isHotkeyRegistered)
                return;

            try
            {
                HotkeyManager.Current.AddOrReplace("ToggleFilter", System.Windows.Input.Key.F12, System.Windows.Input.ModifierKeys.None, OnToggleFilterHotkey);
                isHotkeyRegistered = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to register hotkey: " + ex.Message);
            }
        }

        private void OnToggleFilterHotkey(object? sender, HotkeyEventArgs e)
        {
            if (overlayWindow.IsVisible)
                overlayWindow.Hide();
            else
            {
                ApplyOverlaySettings();
                overlayWindow.Show();
            }

            e.Handled = true;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Hide the window instead of closing
            e.Cancel = true;
            this.Hide();
            NotifyIcon.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ToggleFilter_Click(object sender, RoutedEventArgs e)
        {
            if (overlayWindow.IsVisible)
                overlayWindow.Hide();
            else
            {
                ApplyOverlaySettings();
                overlayWindow.Show();
            }
        }

        private void ManageFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Open filter management window (implementation not shown)
            System.Windows.MessageBox.Show("Filter management feature is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a dialog to add a new schedule (simplified for brevity)
            var newSchedule = new ScheduleEntry
            {
                StartTime = TimeSpan.FromHours(22),
                EndTime = TimeSpan.FromHours(6),
                IntensityCurve = "Exponential"
            };
            schedules.Add(newSchedule);
            scheduler.ScheduleEntries = schedules;
        }
    }
}
