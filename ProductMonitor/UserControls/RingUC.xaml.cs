using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ProductMonitor.UserControls
{
    /// <summary>
    /// RingUC.xaml 的交互逻辑
    /// </summary>
    public partial class RingUC : UserControl
    {
        public RingUC()
        {
            InitializeComponent();

            SizeChanged += ONsizeChanged;
        }

        private void ONsizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRing();
        }


        public void DrawRing()
        {
            double size = Math.Min(RenderSize.Width, RenderSize.Height);
            LayOutGrid.Width = LayOutGrid.Height = size;
            double center = size / 2;
            double radius = center - path.StrokeThickness / 2;
            if (radius <= 0 || !double.IsFinite(PercentValue) || PercentValue <= 0)
            {
                path.Data = null;
                return;
            }

            if (PercentValue >= 100)
            {
                path.Data = new EllipseGeometry(new Point(center, center), radius, radius);
                return;
            }

            double angle = (PercentValue * 3.6 - 90) * Math.PI / 180;
            double x = center + radius * Math.Cos(angle);
            double y = center + radius * Math.Sin(angle);
            int isLargeArc = PercentValue > 50 ? 1 : 0;
            string pathstr = FormattableString.Invariant(
                $"M {center},{center - radius} A {radius},{radius} 0 {isLargeArc} 1 {x},{y}");
            path.Data = Geometry.Parse(pathstr);
        }
        public double PercentValue
        {
            get { return (double)GetValue(PercentValueProperty); }
            set { SetValue(PercentValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PercentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentValueProperty =
            DependencyProperty.Register(nameof(PercentValue), typeof(double), typeof(RingUC),
                new PropertyMetadata(0d, (sender, _) => ((RingUC)sender).DrawRing()));



    }
}
