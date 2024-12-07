using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Forms; // For Screen
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LuminaGuard
{
    public partial class OverlayWindow : Window
    {
        private List<OverlayWindowInstance> overlayInstances = new List<OverlayWindowInstance>();

        public OverlayWindow()
        {
            InitializeComponent();
            Loaded += OverlayWindow_Loaded;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustToAllScreens();
        }

        public void SetOverlayColor(Color color)
        {
            foreach (var overlay in overlayInstances)
            {
                overlay.SetOverlayColor(color);
            }
        }

        private void AdjustToAllScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                var overlay = new OverlayWindowInstance(screen);
                overlay.Hide();
                overlayInstances.Add(overlay);
            }
            this.Hide(); // Hide the initial overlay
        }

        public void ShowAllOverlays()
        {
            foreach (var overlay in overlayInstances)
            {
                overlay.Show();
            }
        }

        public void HideAllOverlays()
        {
            foreach (var overlay in overlayInstances)
            {
                overlay.Hide();
            }
        }

        public bool AnyOverlayVisible()
        {
            foreach (var o in overlayInstances)
            {
                if (o.IsVisible) return true;
            }
            return false;
        }
    }

    public class OverlayWindowInstance : Window
    {
        private Grid OverlayGrid;

        public OverlayWindowInstance(Screen screen)
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;

            Left = screen.Bounds.Left / GetDpiFactor();
            Top = screen.Bounds.Top / GetDpiFactor();
            Width = screen.Bounds.Width / GetDpiFactor();
            Height = screen.Bounds.Height / GetDpiFactor();

            OverlayGrid = new Grid { Background = Brushes.Transparent };
            Content = OverlayGrid;

            Loaded += OverlayWindowInstance_Loaded;
        }

        public void SetOverlayColor(Color color)
        {
            OverlayGrid.Background = new SolidColorBrush(color);
        }

        private void OverlayWindowInstance_Loaded(object sender, RoutedEventArgs e)
        {
            MakeWindowTransparent();
        }

        private void MakeWindowTransparent()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        private double GetDpiFactor()
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}
