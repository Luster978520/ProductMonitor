using ProductMonitor.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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


        private IInputElement? _detailReturnFocus;
        private bool _isDetailClosing;

        private void ShowWorkShopDetail(WorkShopModel workShop)
        {
            if (DetailLayer.Visibility == Visibility.Visible) return;

            _detailReturnFocus = Keyboard.FocusedElement;
            DetailPanel.DataContext = workShop;
            DashboardContent.IsEnabled = false;
            DetailLayer.Visibility = Visibility.Visible;
            // 从 Collapsed 恢复后先完成布局，同一帧内就能使用本次的实际高度。
            DetailLayer.UpdateLayout();
            CloseDetailButton.Focus();
            var slideIn = new DoubleAnimation(DetailPanel.ActualHeight, 0, TimeSpan.FromMilliseconds(520))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
            DetailOffset.BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        private void CloseDetail_Click(object sender, RoutedEventArgs e) => HideWorkShopDetail();

        private void DetailLayer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                HideWorkShopDetail();
            }
        }

        private void HideWorkShopDetail()
        {
            if (_isDetailClosing) return;
            _isDetailClosing = true;

            // 从当前动画位置退回底边，打开途中按 Esc 也不会跳回原点。
            var slideOut = new DoubleAnimation(DetailOffset.Y, DetailPanel.ActualHeight, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            slideOut.Completed += (_, _) =>
            {
                DetailLayer.Visibility = Visibility.Collapsed;
                DetailOffset.BeginAnimation(TranslateTransform.YProperty, null);
                DetailPanel.DataContext = null;
                //清除本地设置的值，让属性重新按继承等规则取值。
                DashboardContent.IsEnabled = true;
                _isDetailClosing = false;
                if (_detailReturnFocus != null) Keyboard.Focus(_detailReturnFocus);
                _detailReturnFocus = null;
            };
            DetailOffset.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }



        #region  //顶部窗口
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
        #endregion
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
