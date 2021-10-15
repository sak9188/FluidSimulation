using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FluidSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            InitDrawCanvas();

            timer.Tick += TimeStep;
            timer.Interval = new TimeSpan(0,0,1);
            timer.Start();
        }

        private void InitDrawCanvas()
        {
            //DrawCanvas.VerticalAlignment = VerticalAlignment.Top;
            //DrawCanvas.HorizontalAlignment = HorizontalAlignment.Left;

            // 初始化粒子
            uint particleNum = 50;

            double x = 0, y = 0;
            double radius = 2.5;
            double diameter = radius * 2;

            for (int i = 0; i < particleNum; i++)
            {
                x = 0;
                for (int j = 0; j < particleNum; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.CadetBlue
                    };

                    e.SetPosition(x, y);
                    Canvas.SetBottom(e, y);
                    Canvas.SetLeft(e, x);
                    DrawCanvas.Children.Add(e);

                    x += diameter;
                }
                y += diameter;
            }
        }

        // 时间步长
        private readonly double timeStep = 0.1;

        private void TimeStep(object sender, EventArgs e)
        {
            //Console.WriteLine(sender.ToString());

            // 给所有的粒子增加一个重力
        }

    }
}
