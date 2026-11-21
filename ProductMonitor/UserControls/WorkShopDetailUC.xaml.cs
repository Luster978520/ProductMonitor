using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ProductMonitor.OperateCommand;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProductMonitor.UserControls
{
    /// <summary>
    /// WorkShopDetailUC.xaml 的交互逻辑
    /// </summary>
    public partial class WorkShopDetailUC : UserControl
    {
        public ICommand ShowDetailCommand { get; }
        private bool _isMachineDetailClosing;

        public WorkShopDetailUC()
        {
            ShowDetailCommand = new BaseCommands(showDetail);
            InitializeComponent();
        }

        private void showDetail()
        {
            if (detail.Visibility == Visibility.Visible) return;

            detail.Visibility = Visibility.Visible;
            detail.UpdateLayout();

            // 从面板自身的高度滑到原位，缓慢减速。
            var slideIn = new DoubleAnimation(MachineDetailPanel.ActualHeight, 0, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
            MachineDetailOffset.BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        private void CloseMachineDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_isMachineDetailClosing) return;
            _isMachineDetailClosing = true;

            // 从当前位置收回，即使打开动画还没有结束也不会跳动。
            var slideOut = new DoubleAnimation(MachineDetailOffset.Y, MachineDetailPanel.ActualHeight, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            slideOut.Completed += (_, _) =>
            {
                detail.Visibility = Visibility.Collapsed;
                MachineDetailOffset.BeginAnimation(TranslateTransform.YProperty, null);
                _isMachineDetailClosing = false;
            };
            MachineDetailOffset.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }

    }


}
