using ProductMonitor.OperateCommand;
using ProductMonitor.Models;
using ProductMonitor.UserControls;
using System.Windows;

namespace ProductMonitor
{
    public partial class MainWindow : Window
    {
        MainWindowVM mainWindowVM = new MainWindowVM();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainWindowVM;
            mainWindowVM.WorkShopDetailRequested += ShowWorkShopDetail; //vm WorkShopDetailRequested订阅事件
        }


        // 窗口创建、所属关系及显示方式属于视图职责。
        private void ShowWorkShopDetail(WorkShopModel workShop)
        {
            var detailWindow = new Window
            {
                Title = $"{workShop.WorkShopName} · 车间详情",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 560,
                Height = 360,
                MinWidth = 420,
                MinHeight = 280,
                ShowInTaskbar = false,
                DataContext = workShop,
                Content = new WorkShopDetailUC()
            };

            // UserControl 不能独立弹出，需要 Window 承载。
            // 每次创建新实例，关闭后可以再次打开。
            detailWindow.ShowDialog();
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
            mainWindowVM.WorkShopDetailRequested -= ShowWorkShopDetail;
            if (DataContext is MainWindowVM vm)
            {
                vm.StopTimer();
            }
            base.OnClosed(e);
        }
    }
}
