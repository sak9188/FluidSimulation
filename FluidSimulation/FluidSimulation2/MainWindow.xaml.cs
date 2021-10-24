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

namespace FluidSimulation2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer drawTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            InitParticles();

            drawTimer.Tick += TimeStep;
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            drawTimer.Start();
        }

        private void InitParticles()
        {
            // 初始化粒子

            InitBoundaries();
        }

        private void InitBoundaries()
        {
            // 初始化边界
        }

        private double GetTheMaxTimeStep(double minTime, double maxTime)
        {
            
        }

        public void TimeStep(object sender, EventArgs e)
        {
            drawTimer.Stop();


            drawTimer.Start();
        }
    }
}
