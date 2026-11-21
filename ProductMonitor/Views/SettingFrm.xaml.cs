using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProductMonitor.UserControls;

namespace ProductMonitor.Views
{
    /// <summary>
    /// SettingFrm.xaml 的交互逻辑
    /// </summary>
    public partial class SettingFrm : Window
    {
        public SettingFrm()
        {
            InitializeComponent();
            SettingsMenu.SelectedIndex = 0;
        }

        private void SettingsMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsMenu.SelectedItem is not ListBoxItem selectedItem) return;

            string title = selectedItem.Content?.ToString() ?? string.Empty;
            ContentTitle.Text = title;
            // 设备清单使用独立控件，其他页面暂时显示占位内容。
            SettingsContent.Content = SettingsMenu.SelectedIndex == 0
                ? new DeviceListUC()
                : SettingsMenu.SelectedIndex == 3 ? new ModbusConfigUC() : $"{title}的配置内容将在这里显示。";


            //SettingsContent.Content=SettingsMenu.SelectedIndex==1?
            //    new DeviceConfigUC() : SettingsMenu.SelectedIndex == 3 ? new ModbusConfigUC() : $"{title}的配置内容将在这里显示。";
        }
    }
}

