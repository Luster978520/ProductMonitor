using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProductMonitor.Models;
using ProductMonitor.OperateCommand;
using ProductMonitor.UserControls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
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
                 TextSize = 11,
                 MinStep = 100,         // 刻度间隔设为 100
                 ForceStepToMin = true  // 强制使用 MinStep，不让图表自动改成 200
             }
        };



        //数据
        public ObservableCollection<int> ProductionValues2 { get;
        } = new ObservableCollection<int> { 23, 23, 54, 67, 23, 54};

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

        // 五项资源保障评分（0～100），顺序必须与 ResourceAngleAxes.Labels 一致。
        public ObservableCollection<double> ResourceValues { get; } = new() { 85, 70, 90, 65, 80 };
        public ISeries[] ResourceSeries { get; }

        // 角度轴：决定圆周上的五个维度；每隔一个数据位置显示一个标签。
        public PolarAxis[] ResourceAngleAxes { get; } =
        {
            new PolarAxis
            {
                Labels = new[] { "算力", "内存", "存储", "网络", "能源" },
                MinStep = 1,
                ForceStepToMin = true,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#334155"))
                {
                    SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")
                },
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#CBD5E1")) { StrokeThickness = 1 }
            }
        };

        // 半径轴：离中心越远，分数越高；固定量程，避免数据变化时自动缩放。
        public PolarAxis[] ResourceRadiusAxes { get; } =
        {
            new PolarAxis
            {
                MinLimit = 0,
                MaxLimit = 100,
                MinStep = 20,
                ForceStepToMin = true,
                TextSize = 9,
                Labeler = value => value.ToString("0"),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E2E8F0")) { StrokeThickness = 1 }
            }
        };

        #endregion

        private WorkShopModel _workShopModel;

        public WorkShopModel WorkShop
        {
            get { return _workShopModel; }
            set { _workShopModel = value; }
        }

        

        #region
        public BaseCommands ShowDetailCmd { get; }

        // VM 只发出打开请求，不创建 Window 或 UserControl。
        public event Action<WorkShopModel>? WorkShopDetailRequested;
        //Action<WorkShopModel> 委托签名，要求订阅的事件必须是带一个参数《WorkShopModel》

        //事件，只能在类的内部创建，并且在类的内部执行，外部只能订阅
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

            // 一个系列包含五个点，依次连接并闭合后形成雷达图。
            ResourceSeries = new ISeries[]
            {
                new PolarLineSeries<double>
                {
                    Name = "资源保障评分",
                    Values = ResourceValues,
                    IsClosed = true, // 把最后一个点连回第一个点。
                    LineSmoothness = 0, // 使用直线，保留雷达图的棱角。
                    Stroke = new SolidColorPaint(SKColor.Parse("#0284C7")) { StrokeThickness = 2 },
                    Fill = new SolidColorPaint(new SKColor(2, 132, 199, 55)),
                    GeometrySize = 7,
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#0284C7")) { StrokeThickness = 2 }
                }
            };

            #endregion


          #region  环境初始化赋值
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

            #endregion



            // 初始化定时器，间隔1秒
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 先赋值一次，避免打开界面空白一秒
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");


            StaffResourceList = new ObservableCollection<PeopleResource> {
            new PeopleResource{staffName="张三", Position="经理", OutWorkDay=23},
            new PeopleResource{staffName="李四", Position="经理", OutWorkDay=12},
            new PeopleResource{staffName="王五", Position="经理", OutWorkDay=8},
            new PeopleResource{staffName="王小芃", Position="经理", OutWorkDay=18},
            };

            _workShopList = new ObservableCollection<WorkShopModel> {
            new WorkShopModel{ WorkShopName="贴片车间",WorkShopCount=32,WaitCount=8,WrongCount=4,StopCount=5,Totle=44},
            new WorkShopModel{ WorkShopName="封装车间",WorkShopCount=98,WaitCount=3,WrongCount=0,StopCount=2,Totle=93},
            new WorkShopModel{ WorkShopName="接焊车间",WorkShopCount=54,WaitCount=2,WrongCount=3,StopCount=5,Totle=23},
            new WorkShopModel{ WorkShopName="密封车间",WorkShopCount=24,WaitCount=1,WrongCount=2,StopCount=1,Totle=54},
            };
            ShowDetailCmd = new BaseCommands(showDetailMethod);




       

        #region
        MachineList = new ObservableCollection<MachineModel>();
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                int plan = random.Next(100,1000);
                int complete = random.Next(0,plan);
                MachineList.Add(new MachineModel
                {
                    MachineName = "焊接机-" +(i+1),
                    FinshedCount=complete,
                    PlanCount=plan,
                    MachineStatus="作业中",
                    OrderNo = "132123123"
                   
                });
            }

            #endregion
        }

        public ISeries[] Series2 { get; set; }
                 = new ISeries[]
                 {
                new LineSeries<int>
                {
                    Values = new int[] { 4, 6, 5, 3, -3, -1, 2 }
                },
                new ColumnSeries<double>
                {
                    Values = new double[] { 2, 5, 4, -2, 4, -3, 5 }
                }
                 };
        //方法不执行  当命令被执行的时候执行，该方法作为参数传递给BaseCommand(Action(object) action);
        //该方法执行后触发事件workshopDetailRequested
        private void showDetailMethod(object? parameter)
        {
            if (parameter is WorkShopModel workShop)
            {
                WorkShopDetailRequested?.Invoke(workShop); //类的内部执行事件的调用
                _workShopModel = workShop;
            }
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



        #region  //人力资源
        private ObservableCollection<PeopleResource> _peopleResourceList;

        public ObservableCollection<PeopleResource> StaffResourceList
        {
            get { return _peopleResourceList; }
            set { _peopleResourceList = value;
                OnPropertyChanged(nameof(StaffResourceList));
            }
        }
        #endregion


        #region
        //车间数据
        private ObservableCollection<WorkShopModel> _workShopList;

        public ObservableCollection<WorkShopModel> WorkShopList
        {
            get { return _workShopList; }
            set {
                _workShopList = value;
                OnPropertyChanged(nameof(WorkShopList));
            }
        }

        #endregion


        private ObservableCollection<MachineModel> _machineList;

        public ObservableCollection<MachineModel> MachineList
        {
            get { return _machineList; }
            set { _machineList = value;
                OnPropertyChanged(nameof(MachineList));
            }
        }


    



    }


}
