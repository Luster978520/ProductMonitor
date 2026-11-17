using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Data.SqlClient;



namespace testSerial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged,ICommand
    {


       
        

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            token = cts.Token;
           
        }

        DeviceResult result = new DeviceResult {
            DeviceCode = "DEV001",
            Barcode = "SN202611130001",
            Temperature = 25.6,
            IsPassed = true
        };



        private string _showText;
        private Thread t1 = new Thread(testThreadMethod);

        private static void testThreadMethod(object? obj)
        {
          
        }

        int number = 0;
        private async Task MonitorDericeAsync(CancellationToken token) {

            token = cts.Token;
            while (!token.IsCancellationRequested)
            {
                number++;
               await Task.Delay(1000);
                
                    showText2.Text = "test+"+number+"\r\n";
               
            }
        }


        private  void testMethod()
        {
            while (true)
            {
                TestMessage += "1" + "/r/n";
            }
        }

        private string _testMessage="wwww";

        public string? TestMessage
        {
            get { return _testMessage; }
            set { _testMessage = value;
                OnPropertyChanged2(TestMessage);
            }
        }

        public void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        public void OnPropertyChanged2([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(
              this,
              new PropertyChangedEventArgs(propertyName)
          );
        }

        //事件 委托类型签名规定了事件的方法类型   事件
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        public string ShowText
        {
            get { return _showText; }
            set { _showText = value;
                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowText)));
            }
        }

        //string json = JsonSerializer.Serialize(result);
        //实例字段的初始化表达式不能直接引用当前对象的其他实例字段。
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowText = JsonSerializer.Serialize(
        result,
        new JsonSerializerOptions { WriteIndented = true });

            DeviceResult? JsonResult= JsonSerializer.Deserialize<DeviceResult>(ShowText);

            ShowText += JsonResult?.ToString();
           
        }

       
        private async void btn_TestThreadClick(object sender, RoutedEventArgs e) {
              await   MonitorDericeAsync(token);
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cts.Cancel(); 
        }
    }
}