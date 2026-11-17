using System.Windows;

namespace ProductMonitor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVM();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            // 读取实际窗口状态，兼容双击标题栏及系统快捷键造成的变化。
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainWindowVM vm)
            {
                vm.StopTimer();
            }
            base.OnClosed(e);
        }
    }
}
