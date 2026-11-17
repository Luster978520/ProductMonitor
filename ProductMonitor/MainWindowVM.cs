using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProductMonitor.Models;
using ProductMonitor.UserControls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProductMonitor
{
    public class MainWindowVM : INotifyPropertyChanged
    {

        #region  //柱状图折线图
        // 示例数据：每个值依次对应 X 轴上的一个时间标签。
        public ObservableCollection<int> ProductionValues { get; } = new() { 300, 400, 550, 340, 600, 450 };
        // 不良数量
        public ObservableCollection<int> DefectValues { get; } = new() { 12, 18, 25, 15, 30, 20 };
        public ISeries[] ProductionSeries { get; }
        public Axis[] ProductionXAxes { get; } =
        {
            new Axis { Labels = new[] { "8:00", "9:00", "10:00", "11:00", "12:00", "13:00" }, MinStep = 1 }
        };

        // 产能图表的 Y 轴
        public Axis[] ProductionYAxes { get; } =
        {
             new Axis
             {
                 MinLimit = 0,          // Y 轴从 0 开始
                 MinStep = 100,         // 刻度间隔设为 100
                 ForceStepToMin = true  // 强制使用 MinStep，不让图表自动改成 200
             }
        };



        //数据
        public ObservableCollection<int> ProductionValues2 { get; } = new ObservableCollection<int> { 23, 23, 54, 67, 23, 54};

        //x轴
        public Axis[] ProductionXAxes2 { get; } =
        {
            new Axis{Labels=new[]{"axis1","axis2","axis3","axis4","axis5", "axis6" },
            },
        };

        public Axis[] ProductionYAxes2 { get; } = new Axis[]
        {
            new Axis{
            MinLimit=10,
            MinStep=5,
            MaxLimit=80
            
            }
        };

        // 用同一个阈值生成线的位置和文字，避免两者不一致。
        private const double QualityAlarmThreshold = 50;
        public RectangularSection[] QualityAlarmSections { get; } =
        {
            new RectangularSection
            {
                Yi = QualityAlarmThreshold,
                Yj = QualityAlarmThreshold,
                Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 },
                Label = $"告警阈值 {QualityAlarmThreshold}",
                LabelPaint = new SolidColorPaint(SKColors.Red),
                LabelSize = 12
            }
        };

        public ISeries[] ProductionSeries2 { get; }

        // 饼图不需要 X/Y 轴：这里每个 PieSeries 对应一个类别，Values 中放一个数量。
        public ISeries[] ProductionSeries3 { get; }

        // LiveCharts 用 Skia 绘制图例，指定中文字体以显示类别名称。
        public SolidColorPaint ProductionLegendPaint { get; } = new(SKColors.SlateGray)
        {
            SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")
        };

        #endregion


        #region
        //报警记录
        private ObservableCollection<AlarmRecord> _alarmRecordList;

        public ObservableCollection<AlarmRecord> AlarmRecordList
        {
            get { return _alarmRecordList; }
            set { _alarmRecordList = value; }
        }


        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");



        #region 显示时间 计数
        /// <summary>
        /// 当前时间，绑定到界面
        /// </summary>
        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }


        private string _nowDateTime;
        public string NowDateTime
        {
            get => _nowDateTime;
            set
            {
                _nowDateTime = value;
                OnPropertyChanged();
            }
        }


        private string _machineCount = "0298";

        public string MachineCount
        {
            get { return _machineCount; }
            set
            {
                _machineCount = value;
                OnPropertyChanged(nameof(MachineCount));
            }
        }


        private readonly DispatcherTimer _timer;

        #endregion
        // 实现属性变更通知
        public MainWindowVM()
        {
          # region  柱状图折线图
            ProductionSeries = new ISeries[]
            {
                new ColumnSeries<int> { Name = "生产计数", Values = ProductionValues, MaxBarWidth = 10 ,Tag="长产技术" ,
                
                },
                 new ColumnSeries<int>{Name = "不良数量",Values = DefectValues, MaxBarWidth = 10 }
            };


            ProductionSeries2 = new ISeries[] {
                new LineSeries<int>{Name="质量",Values=ProductionValues2, GeometrySize=0}
            };

            // 示例总数 1000：合格 70%、待检 20%、不良 10%。扇区大小由控件自动计算。
            ProductionSeries3 = new ISeries[]
            {
                new PieSeries<int>
                {
                    Name = "合格",
                    Values = new ObservableCollection<int> { 700 },
                    Fill = new SolidColorPaint(SKColor.Parse("#0284C7")),
                    InnerRadius = 0, // 0 为实心饼图；改成 50 可观察圆环效果。
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0")
                },
                new PieSeries<int>
                {
                    Name = "待检",
                    Values = new ObservableCollection<int> { 200 },
                    Fill = new SolidColorPaint(SKColor.Parse("#D97706")),
                    InnerRadius = 0,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0")
                },
                new PieSeries<int>
                {
                    Name = "不良",
                    Values = new ObservableCollection<int> { 100 },
                    Fill = new SolidColorPaint(SKColor.Parse("#DC2626")),
                    InnerRadius = 0,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("0")
                }
            };

          #endregion
            //构造函数添加初始化环境监控数据
            _EnvironmentList = new List<Environments>{
                new Environments { EnItemName = "光照(Lux)", EnItemValue = 123 },
                new Environments { EnItemName = "噪音(db)", EnItemValue = 55 },
                new Environments { EnItemName = "温度(摄氏度)", EnItemValue = 123 },
                new Environments { EnItemName = "湿度(%)", EnItemValue = 123 },
                new Environments { EnItemName = "PM2.5(立方米）", EnItemValue = 20 },
                new Environments { EnItemName = "硫化氢", EnItemValue = 123 },
                new Environments { EnItemName = "氮气(PPM)", EnItemValue = 18 },
                new Environments { EnItemName = "氮气", EnItemValue = 123 },
            };

            _alarmRecordList = new ObservableCollection<AlarmRecord>
            {
                new AlarmRecord{SN=1,Alarm="温度过高", Time=DatatimeNow, DuringTime="20min"},
                new AlarmRecord{SN=2,Alarm="温度过高", Time=DatatimeNow, DuringTime="20min"},
                new AlarmRecord{SN=3,Alarm="温度过高", Time=DatatimeNow, DuringTime="20min"},
                new AlarmRecord{SN=4,Alarm="温度过高", Time=DatatimeNow, DuringTime="20min"},
                new AlarmRecord{SN=5,Alarm="温度过高", Time=DatatimeNow, DuringTime="20min"},
            };

            // 初始化定时器，间隔1秒
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 先赋值一次，避免打开界面空白一秒
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }


        public void StopTimer()
        {
            _timer?.Stop();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            UpdateTime();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



          #region 自定义控件显示
        private UserControl _monitorUC;

        public UserControl MonitorUC
        {
            get
            {
                if (_monitorUC == null)
                {
                    _monitorUC = new MonitorUC();
                }
                return _monitorUC;
            }
            set
            {
                _monitorUC = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MonitorUC"));
                }
            }
        }

           #endregion


        private DateTime _datatime = DateTime.Now;

        public DateTime DatatimeNow
        {
            get { return _datatime; }
            set { _datatime = value; }
        }



        private void UpdateTime()
        {
            DateTime now = DateTime.Now;
            // 格式：2026-11-13 14:30:25 星期五
            NowDateTime = GetWeekName(now.DayOfWeek);
        }


        private string GetWeekName(DayOfWeek week)
        {
            return week switch
            {
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                DayOfWeek.Sunday => "星期日",
                _ => ""
            };
        }



        #region 环境监控数据
        private List<Environments> _EnvironmentList;

        public List<Environments> EnvironmentList
        {
            get { return _EnvironmentList; }
            set
            {
                if (ReferenceEquals(_EnvironmentList, value))
                    return;

                _EnvironmentList = value;
                OnPropertyChanged();
            }
        }


        #endregion




    }


}
