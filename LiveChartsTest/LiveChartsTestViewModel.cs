using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LiveChartsTest
{
    public class LiveChartsTestViewModel
    {
        public class PieData(string name, double value, double pushout)
        {
            public string Name { get; set; } = name;
            public double[] Values { get; set; } = [value];
            public double Pushout { get; set; } = pushout;
        }

        public PieData[] Data { get; set; } = [
        new("Maria",    8, 30),
        new("Susan",    6, 0),
        new("Charles",  5, 0),
        new("Fiona",    3, 0),
        new("George",   3, 0)
    ];




        public class PieData2(string name, double value)
        {
            public string Name { get; set; } = name;
            public double[] Values { get; set; } = [value];
        }

        public PieData2[] Data2 { get; set; } = [
            new("Mary", 10),
            new("John", 20),
            new("Alice", 30),
            new("Bob", 40),
            new("Charlie", 50)
        ];




        // 保留数据集合的引用，按钮点击时直接 Add；集合会通知图表刷新。
        public ObservableCollection<double> PolarValues { get; } = new() { 5, 2, 1, 4, 3, 3 };

        public ObservableCollection<ISeries> Series { get; }

        public LiveChartsTestViewModel()
        {
            Series = new ObservableCollection<ISeries>
            {
                new PolarLineSeries<double>
                {
                    Values = PolarValues,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    // 必须指定描边（Stroke）或填充（Fill），否则图表不可见
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                    Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(90)), // 半透明填充（雷达图常用）
                    IsClosed = true,
                },
                new PolarLineSeries<double>
                {
                    Values = new ObservableCollection<double> { 3, 5, 2, 3, 4, 5 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0.2,
                    Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
                    Fill = new SolidColorPaint(SKColors.OrangeRed.WithAlpha(90)),
                    IsClosed = true,
                }
            };
        }

        public void AddPolarData()
        {
            // double 数据默认按索引对应角度：第七个值对应第七个标签。
            // 重新赋值 Labels，让坐标轴收到标签属性的变更通知。
            AngleAxes[0].Labels = AngleAxes[0].Labels!
                .Append($"item {PolarValues.Count + 1}").ToArray();

            // Next 的上限不包含在结果内，因此这里生成 1～6。
            PolarValues.Add(Random.Shared.Next(1, 7));
        }

        public PolarAxis[] RadiusAxes { get; set; }
           = new PolarAxis[]  
           {
                new PolarAxis
                {
                    // formats the value as a number with 2 decimals.
                    Labeler = value => value.ToString("N2"),
                    // 半径轴的分隔线是同心圆环。
                    SeparatorsPaint = new SolidColorPaint(SKColors.DimGray)
                    {
                        StrokeThickness = 1.5f
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray)
                }
           };

        public PolarAxis[] AngleAxes { get; set; }
            = new PolarAxis[]
            {
                new PolarAxis
                {
                    Labels = new[] { "strength", "stamina", "resistance", "power", "hit points", "mana" },
                    MinStep = 1,
                    ForceStepToMin = true,
                    // 角度轴的分隔线是由中心向外的放射线。
                    SeparatorsPaint = new SolidColorPaint(SKColors.DimGray)
                    {
                        StrokeThickness = 1.5f
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray)
                }
            };



    }
}
