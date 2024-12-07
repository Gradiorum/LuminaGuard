using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using WF = System.Windows.Forms;
using LuminaGuard.Helpers;
using System.Windows.Controls;
using LuminaGuard.Models;
using System.Linq;
using System.Diagnostics;

namespace LuminaGuard
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlayWindow;
        private DispatcherTimer schedulerTimer = new DispatcherTimer();
        private Scheduler scheduler;
        private BrightnessController? brightnessController;
        private FilterManager filterManager;
        private ObservableCollection<ScheduleEntry> schedules;
        private bool isHotkeyRegistered = false;
        private SmartLightingController? lightingController;
        private DispatcherTimer pauseCheckTimer = new DispatcherTimer();
        private DateTime pauseEndTime = DateTime.MinValue;
        private bool isPaused = false;
        private ProcessMonitor processMonitor;
        private bool uiLoaded = false; // Ensure UI loaded before applying settings

        public MainWindow()
        {
            InitializeComponent();
            schedules = new ObservableCollection<ScheduleEntry>();
            // Defer UI-dependent initialization until Loaded event
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize components that depend on UI
            overlayWindow = new OverlayWindow();
            scheduler = new Scheduler();
            brightnessController = new BrightnessController();
            filterManager = new FilterManager();
            processMonitor = new ProcessMonitor();

            ScheduleListBox.ItemsSource = schedules;

            ProfilesComboBox.ItemsSource = App.ProfilesManager.GetProfileNames();
            if (!string.IsNullOrEmpty(App.CurrentConfig.CurrentProfileName))
                ProfilesComboBox.SelectedItem = App.CurrentConfig.CurrentProfileName;
            else
                ProfilesComboBox.SelectedItem = App.ProfilesManager.GetProfileNames().FirstOrDefault();

            InitializeScheduler();
            RegisterHotkeys();
            InitializeSmartLightingOptions();
            ApplyConfigToUI();

            pauseCheckTimer.Interval = TimeSpan.FromSeconds(5);
            pauseCheckTimer.Tick += PauseCheckTimer_Tick;
            pauseCheckTimer.Start();

            EnableSchedulerCheckBox.Checked += EnableSchedulerCheckBox_Checked;
            EnableSchedulerCheckBox.Unchecked += EnableSchedulerCheckBox_Unchecked;

            if (!App.CurrentConfig.HideNightLightWarning)
            {
                MessageBox.Show("For optimal performance, please disable Windows Night Light and close f.lux if running.",
                            "LuminaGuard", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            uiLoaded = true;
            if (App.CurrentConfig.FilterEnabled && !isPaused) ApplyOverlaySettings();
        }

        private void PauseCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (isPaused)
            {
                if (PauseForDurationRadio.IsChecked == true && DateTime.Now > pauseEndTime && pauseEndTime != DateTime.MinValue)
                {
                    ResumeNow();
                }
                else if (PauseUntilAppsCloseRadio.IsChecked == true)
                {
                    var apps = AppsToMonitorTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(a => a.Trim()).ToList();
                    if (apps.Count > 0)
                    {
                        bool anyRunning = processMonitor.AnyProcessRunning(apps);
                        if (!anyRunning)
                        {
                            ResumeNow();
                        }
                    }
                }
            }
            else
            {
                if (PauseUntilAppsCloseRadio.IsChecked == true)
                {
                    var apps = AppsToMonitorTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(a => a.Trim()).ToList();
                    if (apps.Count > 0)
                    {
                        bool anyRunning = processMonitor.AnyProcessRunning(apps);
                        if (anyRunning)
                        {
                            PauseNow();
                        }
                    }
                }
            }
        }

        private void PauseNow()
        {
            isPaused = true;
            overlayWindow?.HideAllOverlays();
        }

        private void ResumeNow()
        {
            isPaused = false;
            ApplyOverlaySettings(); 
        }

        private void ApplyConfigToUI()
        {
            // Check for null references since UI elements should be ready by now
            if (App.CurrentConfig == null) return;

            IntensitySlider.Value = App.CurrentConfig.Intensity;
            BrightnessSlider.Value = App.CurrentConfig.Brightness;
            TemperatureSlider.Value = App.CurrentConfig.ColorTemperature;
            EnableFilterCheckBox.IsChecked = App.CurrentConfig.FilterEnabled;
            HexCodeTextBox.Text = App.CurrentConfig.CustomColorHex;
            EnableSchedulerCheckBox.IsChecked = App.CurrentConfig.SchedulerEnabled;
            EnableBrightnessAdjustmentCheckBox.IsChecked = App.CurrentConfig.BrightnessOverTimeEnabled;
            EnableSmartLightingCheckBox.IsChecked = App.CurrentConfig.SmartLightingEnabled;
            EnableMLCheckBox.IsChecked = App.CurrentConfig.MLAdaptationEnabled;
            DontShowNightLightWarningCheckBox.IsChecked = App.CurrentConfig.HideNightLightWarning;

            schedules.Clear();
            if (App.CurrentConfig.Schedules != null)
            {
                foreach (var sch in App.CurrentConfig.Schedules)
                {
                    schedules.Add(sch);
                }
                scheduler.ScheduleEntries = schedules;
            }

            if (!string.IsNullOrWhiteSpace(App.CurrentConfig.AccessibilityMode))
            {
                AccessibilityComboBox.SelectedItem = AccessibilityComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content?.ToString() == App.CurrentConfig.AccessibilityMode);
            }
        }

        private void InitializeSmartLightingOptions()
        {
            SmartLightsComboBox.Items.Clear();
            // Populate with mock options, in a real scenario we could dynamically load plugins
            SmartLightsComboBox.Items.Add("Hue Bridge");
            SmartLightsComboBox.Items.Add("LIFX");
            SmartLightsComboBox.Items.Add("HomeKit");
        }

        private void EnableFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.FilterEnabled = (EnableFilterCheckBox.IsChecked == true);
            if (App.CurrentConfig.FilterEnabled && !isPaused)
            {
                ApplyOverlaySettings();
                overlayWindow?.ShowAllOverlays();
            }
            else
            {
                overlayWindow?.HideAllOverlays();
            }
        }

        private void IntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.Intensity = IntensitySlider.Value;
            if (!isPaused) ApplyOverlaySettings();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!uiLoaded) return;
            if (brightnessController == null) return;
            byte brightness = (byte)BrightnessSlider.Value;
            App.CurrentConfig.Brightness = brightness;
            try
            {
                brightnessController.SetBrightness(brightness);
            }
            catch (Exception ex)
            {
                Logging.Log("Error adjusting brightness: " + ex.ToString());
            }
        }

        private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.ColorTemperature = TemperatureSlider.Value;
            HexCodeTextBox.Text = ""; 
            if (!isPaused) ApplyOverlaySettings();
        }

        private void HexCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.CustomColorHex = HexCodeTextBox.Text;
            if (!isPaused) ApplyOverlaySettings();
        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new WF.ColorDialog();
            if (colorDialog.ShowDialog() == WF.DialogResult.OK)
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
            if (!uiLoaded || overlayWindow == null || isPaused) return;
            if (!App.CurrentConfig.FilterEnabled) return;

            Color color;

            if (IsValidHex(HexCodeTextBox.Text))
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(HexCodeTextBox.Text);
                }
                catch
                {
                    MessageBox.Show("Invalid hex code entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                double kelvin = TemperatureSlider.Value;
                color = ColorTemperatureConverter.ColorTemperatureToRGB(kelvin);
            }

            double intensity = IntensitySlider.Value / 100.0;
            byte alpha = (byte)(intensity * 255);

            if (alpha >= 230)
            {
                alpha = 230;
            }

            color = Color.FromArgb(alpha, color.R, color.G, color.B);

            filterManager.SetAccessibilityMode(App.CurrentConfig.AccessibilityMode);
            color = filterManager.ApplyFilters(color);

            overlayWindow.SetOverlayColor(color);

            if (App.CurrentConfig.SmartLightingEnabled && lightingController != null)
            {
                try
                {
                    lightingController.SetColorTemperature(TemperatureSlider.Value);
                }
                catch (Exception ex)
                {
                    Logging.Log("Error setting smart lighting temperature: " + ex.ToString());
                }
            }
        }

        private void InitializeScheduler()
        {
            schedulerTimer.Interval = TimeSpan.FromMinutes(1);
            schedulerTimer.Tick += SchedulerTimer_Tick;
            if (App.CurrentConfig.SchedulerEnabled)
                schedulerTimer.Start();
        }

        private void SchedulerTimer_Tick(object? sender, EventArgs e)
        {
            if (EnableSchedulerCheckBox.IsChecked == true)
            {
                UpdateIntensityBasedOnSchedule();
            }

            if (App.CurrentConfig.MLAdaptationEnabled)
            {
                MLAdaptiveEngine.AdaptSettings(App.CurrentConfig);
                if (!isPaused) ApplyOverlaySettings();
            }
        }

        private void UpdateIntensityBasedOnSchedule()
        {
            DateTime now = DateTime.Now;
            double progress = scheduler.CalculateProgress(now.TimeOfDay);

            double scheduledIntensity = progress * IntensitySlider.Maximum;
            IntensitySlider.Value = scheduledIntensity;

            if (EnableBrightnessAdjustmentCheckBox.IsChecked == true && brightnessController != null)
            {
                byte brightness = (byte)(BrightnessSlider.Maximum - scheduledIntensity);
                brightnessController.SetBrightness(brightness);
                App.CurrentConfig.Brightness = brightness;
            }
        }

        private void EnableSchedulerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.SchedulerEnabled = true;
            schedulerTimer.Start();
        }

        private void EnableSchedulerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.SchedulerEnabled = false;
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
                MessageBox.Show("Failed to register hotkey: " + ex.Message);
            }
        }

        private void OnToggleFilterHotkey(object? sender, HotkeyEventArgs e)
        {
            if (isPaused)
            {
                e.Handled = true;
                return;
            }

            if (overlayWindow != null)
            {
                if (overlayWindow.AnyOverlayVisible())
                    overlayWindow.HideAllOverlays();
                else
                {
                    ApplyOverlaySettings();
                    overlayWindow.ShowAllOverlays();
                }
            }

            e.Handled = true;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            if (NotifyIcon != null)
                NotifyIcon.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ToggleFilter_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused || overlayWindow == null) return;

            if (overlayWindow.AnyOverlayVisible())
                overlayWindow.HideAllOverlays();
            else
            {
                ApplyOverlaySettings();
                overlayWindow.ShowAllOverlays();
            }
        }

        private void ManageFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Filter management feature placeholder. Future versions could allow loading filter plugins or editing custom curves.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var newSchedule = new ScheduleEntry
            {
                StartTime = TimeSpan.FromHours(22),
                EndTime = TimeSpan.FromHours(6),
                IntensityCurve = "Exponential"
            };
            schedules.Add(newSchedule);
            scheduler.ScheduleEntries = schedules;
            App.CurrentConfig.Schedules = schedules.ToList();
        }

        private void ProfilesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedName = ProfilesComboBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedName))
            {
                App.ProfilesManager.SwitchProfile(selectedName);
                ApplyConfigToUI();
                if (!isPaused) ApplyOverlaySettings();
            }
        }

        private void NewProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string newProfileName = "NewProfile" + DateTime.Now.Ticks;
            App.ProfilesManager.CreateProfile(newProfileName);
            ProfilesComboBox.ItemsSource = App.ProfilesManager.GetProfileNames();
            ProfilesComboBox.SelectedItem = newProfileName;
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profName = ProfilesComboBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(profName))
            {
                App.ProfilesManager.DeleteProfile(profName);
                ProfilesComboBox.ItemsSource = App.ProfilesManager.GetProfileNames();
                ProfilesComboBox.SelectedItem = App.ProfilesManager.GetProfileNames().FirstOrDefault();
            }
        }

        private void OpenTipsButton_Click(object sender, RoutedEventArgs e)
        {
            var tipsWindow = new Views.TipsAndTricksWindow();
            tipsWindow.Show();
        }

        private void CloudSyncButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloudSyncService.Sync(App.CurrentConfig);
                MessageBox.Show("Settings synchronized to cloud.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logging.Log("Cloud sync error: " + ex.ToString());
                MessageBox.Show("Failed to sync settings to cloud.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnableSmartLightingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.SmartLightingEnabled = true;
            if (SmartLightsComboBox.SelectedItem != null)
            {
                var selectedLight = SmartLightsComboBox.SelectedItem.ToString();
                if (selectedLight != null)
                {
                    if (selectedLight.Contains("Hue"))
                        lightingController = new HueAdapter();
                    else if (selectedLight.Contains("LIFX"))
                        lightingController = new LIFXAdapter();
                    else
                        lightingController = new HueAdapter(); // fallback

                    try
                    {
                        lightingController.Initialize();
                        lightingController.SetColorTemperature(TemperatureSlider.Value);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Smart lighting init error: " + ex.ToString());
                    }
                }
            }
        }

        private void EnableSmartLightingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.SmartLightingEnabled = false;
            lightingController = null;
        }

        private void EnableMLCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.MLAdaptationEnabled = true;
        }

        private void EnableMLCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.MLAdaptationEnabled = false;
        }

        private void ApplyPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PauseForDurationRadio.IsChecked == true)
            {
                if (double.TryParse(PauseDurationTextBox.Text, out double hours))
                {
                    pauseEndTime = DateTime.Now.AddHours(hours);
                    PauseNow();
                }
                else
                {
                    MessageBox.Show("Invalid duration input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (PauseUntilManuallyEnabledRadio.IsChecked == true)
            {
                pauseEndTime = DateTime.MinValue;
                PauseNow();
            }
            else if (PauseUntilAppsCloseRadio.IsChecked == true)
            {
                var apps = AppsToMonitorTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(a => a.Trim()).ToList();
                bool anyRunning = processMonitor.AnyProcessRunning(apps);
                if (anyRunning)
                {
                    PauseNow();
                }
                else
                {
                    MessageBox.Show("None of the specified apps are currently running. LuminaGuard will pause automatically when they start.",
                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Select a pause method first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ResumeNowButton_Click(object sender, RoutedEventArgs e)
        {
            ResumeNow();
        }

        private void DisableNightLightButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To disable Night Light:\n1. Open Windows Settings > System > Display.\n2. Toggle off 'Night light'.",
                            "Disable Night Light Instructions", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DontShowNightLightWarningCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!uiLoaded) return;
            App.CurrentConfig.HideNightLightWarning = (DontShowNightLightWarningCheckBox.IsChecked == true);
        }
    }
}
