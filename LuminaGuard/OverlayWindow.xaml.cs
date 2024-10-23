// OverlayWindow.xaml.cs
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls; // For Grid
using System.Windows.Forms;   // For Screen

namespace LuminaGuard
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
            Loaded += OverlayWindow_Loaded;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Adjust to all screens
            AdjustToAllScreens();
        }

        public void SetOverlayColor(Color color)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is OverlayWindowInstance overlay)
                {
                    overlay.OverlayGrid.Background = new SolidColorBrush(color);
                }
            }
        }

        private void AdjustToAllScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                var overlay = new OverlayWindowInstance(screen);
                overlay.Show();
            }
            this.Hide(); // Hide the initial overlay
        }
    }

    public class OverlayWindowInstance : Window
    {
        public Grid OverlayGrid { get; set; }

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
