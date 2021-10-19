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

        // 长宽都是1000, 1000, 单个格子的长度为10, 总计 100*100
        public const int SpaceStep = 10;
        public Dictionary<int, HashSet<Particle>> SpaceDict;

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
                    // 设置空间映射
                    var xPos = Convert.ToInt32(x / SpaceStep);
                    var yPos = Convert.ToInt32(y / SpaceStep);

                    var key = xPos + yPos * 100;
                    HashSet<Particle> set = null;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }

                    
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

        private readonly int MaxIteration = 5;

        private double restDensity = 1;

        private double relaxScaler = 1;

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

            // 找到所有的临边
            foreach (var particle in allParticle)
            {
                // 找到所有的相邻的粒子
                // 因为我这里的默认半径是2.5, 所以我觉得光滑核半径应该是半径的两倍
                var xGridPos = Convert.ToInt32(particle.NextPosition.X / SpaceStep);
                var yGridPos = Convert.ToInt32(particle.NextPosition.X / SpaceStep);

                HashSet<Particle> allJoinParticles = new HashSet<Particle>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        HashSet<Particle> hashSet;
                        if (SpaceDict.TryGetValue(xGridPos + yGridPos * 100, out hashSet))
                        {
                            allJoinParticles.UnionWith(hashSet);
                        }
                    }
                }

                particle.NeighborParticles = allJoinParticles;
            }


            for (int i = 0; i < MaxIteration; i++)
            {
                // 计算lambda
                foreach (var particle in allParticle)
                {
                    // 密度约束
                    // Ci = rol_i/rol_0 - 1 = 0
                    // rol_i = sum(J, mj, KernalFunction)
                    double rol_i = 0;
                    double pkc = 0;
                    foreach (var particleNeighborParticle in particle.NeighborParticles)
                    {
                        var kenalGradValue = KernelFunction.Poly6Kernel(particle.NextPosition - particleNeighborParticle.NextPosition,
                            particle.Radius * 2);
;
                        rol_i += kenalGradValue;
                        pkc += Math.Pow(kenalGradValue, 2);
                    }

                    var c_i = rol_i - 1;
                    var lambda = -c_i / (pkc + 1);


                    // 计算det_p
                    // 计算碰撞
                    // 更新位置
                }
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
