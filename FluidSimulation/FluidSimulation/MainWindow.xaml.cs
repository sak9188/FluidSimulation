using System;
using System.CodeDom;
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
            timer.Interval = new TimeSpan(0,0,0,0,Convert.ToInt32(timeStep * 1000));
            timer.Start();
        }

        public List<Particle> allParticle = new List<Particle>();
        public readonly Vector gravity = new Vector(0, -0.98d);
        private void InitDrawCanvas()
        {
            //DrawCanvas.VerticalAlignment = VerticalAlignment.Top;
            //DrawCanvas.HorizontalAlignment = HorizontalAlignment.Left;

            // 初始化粒子
            uint particleNum = 50;

            double x = 0, y = 500;
            double radius = 2.5;
            y += radius;
            double diameter = radius * 2;

            for (int i = 0; i < particleNum; i++)
            {
                x = radius;
                for (int j = 0; j < particleNum; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.DarkKhaki
                    };

                    allParticle.Add(e);

                    e.SetPosition(x, y);

                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y += diameter;
            }
        }

        // 时间步长
        private readonly double timeStep = 0.016;
        // 目标帧数
        private readonly int frames = 1000;

        private int curFrames = 0;

        private Rect wallRect = new Rect(0, 0, 1000, 1000);
        private void TimeStep(object sender, EventArgs e)
        {
            if (curFrames > frames)
            {
                return;
            }

            curFrames += 1;

            // PBD Position Based Dynamics
            // 1. 初始化所有粒子(点Vertices的速度)
            // 2. 计算每个粒子的的每个时间步长的速度
            // 3. 计算下个位置
            // 4. 对每个粒子生成碰撞约束
            // 5. 根据每个约束,反向求解粒子的位置 修正位置
            // 6. 对所有的粒子的位置进行修正, 反向计算速度
            // 7. 更新位置, 更新速度

            // 添加重力
            foreach (var particle in allParticle)
            {
                particle.Force = gravity; 
                particle.PredictPosition(timeStep);

                // particle.SetPosition(particle.Position + particle.NextPosition);
            }

            // 计算pi 和 碰撞检测
            foreach (var particle in allParticle)
            {
                // 这里不计算pi
                // 碰撞检测
                
                // 这里暂时使用O(N^2)的方法
                 
                
            }

            foreach (var particle in allParticle)
            {
                // 更新速度
                particle.Velocity += gravity * timeStep;
                
                // 施加速度限制，和粘力

                // 更新位置
                
            }
        }
    }
}
