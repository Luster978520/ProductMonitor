using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NModbus;
using NModbus.Serial;

namespace ProductMonitor.UserControls;

public partial class ModbusConfigUC : UserControl
{
    private SerialPort? port;
    private IModbusMaster? master;
    private bool busy;
    private bool closePending;

    public ModbusConfigUC()
    {
        InitializeComponent();
        Baud.ItemsSource = new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 }; Baud.SelectedItem = 9600;
        Bits.ItemsSource = new[] { 7, 8 }; Bits.SelectedItem = 8;
        ParityBox.ItemsSource = new[] { Parity.None, Parity.Even, Parity.Odd }; ParityBox.SelectedItem = Parity.None;
        Stops.ItemsSource = new[] { StopBits.One, StopBits.Two }; Stops.SelectedItem = StopBits.One;
        Function.ItemsSource = new[] { "01 读取输出线圈", "02 读取离散输入", "03 读取保持寄存器", "04 读取输入寄存器", "05 写单个线圈", "06 写单个保持寄存器", "15 写多个线圈", "16 写多个保持寄存器" }; Function.SelectedIndex = 0;
        RefreshPorts();
        Unloaded += (_, _) => { if (busy) closePending = true; else Disconnect(); };
    }
    private void RefreshPorts() {
        var selected = Ports.SelectedItem as string; 
        Ports.ItemsSource = SerialPort.GetPortNames().OrderBy(x => x).ToArray();
        Ports.SelectedItem = selected; if (Ports.SelectedItem == null && Ports.Items.Count > 0) Ports.SelectedIndex = 0;
    }
    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshPorts();

    private void Disconnect() { 
        master?.Dispose(); 
        master = null; 
        port?.Dispose(); 
        port = null;
        Status.Text = "未连接"; 
        ConnectButton.Content = "连接"; 
    }
    private void Connect_Click(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        if (master != null)
        { 
            Disconnect(); 
            return; 
        }
        try
        {
            if (Ports.SelectedItem is not string name) 
                throw new System.Exception("请选择串口。");
            // 串口参数必须与从站一致；超时避免设备无响应时无限等待。
            port = new SerialPort(name, (int)Baud.SelectedItem, (Parity)ParityBox.SelectedItem, (int)Bits.SelectedItem, (StopBits)Stops.SelectedItem) { ReadTimeout = 1000, WriteTimeout = 1000 };
            port.Open(); 
            master = new ModbusFactory().CreateRtuMaster(port);
            master.Transport.Retries = 0;
            Status.Text = $"已连接 {name}"; 
            ConnectButton.Content = "断开";
        }
        catch (System.Exception ex) 
        { 
            Disconnect();
            Result.Text = "连接失败：" + ex.Message; 
        }
    }
    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (master == null || busy) 
        { 
            Result.Text = "请先连接串口。";
            return; 
        }
        try
        {
            // 输入地址是 Modbus 报文中的零起始地址，并非手册中的 40001 等显示编号。
            byte slave = byte.Parse(Slave.Text); 
            if (slave < 1 || slave > 247) 
                throw new System.Exception("从站地址必须为 1–247。");
            ushort address = ushort.Parse(Address.Text);
            int op = Function.SelectedIndex;
            ushort count = op <= 3 ? ushort.Parse(Count.Text) : (ushort)1;
            int max = op <= 1 ? 2000 : 125;
            if (op <= 3 && (count < 1 || count > max || (int)address + count > 65536))
                throw new System.Exception($"读取数量或地址范围无效，最大数量为 {max}。");
            string[] parts = op >= 4 ? Values.Text.Split(new[] { ',', '，' }, System.StringSplitOptions.TrimEntries) : System.Array.Empty<string>();
            ushort[] values = parts.Select(ushort.Parse).ToArray();
            if (op >= 4 && (values.Length == 0 || (op <= 5 && values.Length != 1) || (op == 6 && values.Length > 1968) || (op == 7 && values.Length > 123) || address + values.Length > 65536)) 
                throw new System.Exception("写入数量或地址范围无效。");
            if ((op == 4 || op == 6) && values.Any(v => v > 1)) throw new System.Exception("线圈值只能为 0 或 1。");
            var current = master; busy = true; ExecuteButton.IsEnabled = false; ConnectButton.IsEnabled = false;
            // 同步串口请求放到后台线程，避免超时时冻结界面；同一时刻只允许一个请求。
            string output = await Task.Run(() => op switch
            {
                0 => string.Join(", ", current.ReadCoils(slave, address, count).Select(v => v ? "1" : "0")),
                1 => string.Join(", ", current.ReadInputs(slave, address, count).Select(v => v ? "1" : "0")),
                2 => string.Join(", ", current.ReadHoldingRegisters(slave, address, count)),
                3 => string.Join(", ", current.ReadInputRegisters(slave, address, count)),
                4 => Write(() => current.WriteSingleCoil(slave, address, values[0] == 1)),
                5 => Write(() => current.WriteSingleRegister(slave, address, values[0])),
                6 => Write(() => current.WriteMultipleCoils(slave, address, values.Select(v => v == 1).ToArray())),
                7 => Write(() => current.WriteMultipleRegisters(slave, address, values)),
                _ => throw new System.Exception("请选择功能。")
            });
            Result.Text = "成功：" + output;
        }
        catch (System.Exception ex) 
        { 
            Result.Text = "操作失败：" + ex.Message; 
        }
        finally {
            busy = false; 
            ExecuteButton.IsEnabled = true;
            ConnectButton.IsEnabled = true;
            if (closePending) Disconnect(); }
    }
    private static string Write(System.Action action)
    { 
        action(); 
        return "写入完成";
    }
}

