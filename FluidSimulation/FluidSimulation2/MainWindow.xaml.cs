using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        private List<Particle> allParticles = new List<Particle>();
        private List<Particle> fluidParticle = new List<Particle>();
        private List<Particle> solidParticle = new List<Particle>();

        private Vector maxSpeed = new Vector();

        private static readonly int scaler = 10;

        private static readonly double canvasWidth = 800;

        private static readonly double realWidth = canvasWidth / scaler;

        private static readonly double canvasHeight = 800;

        private static readonly double realHeight = canvasHeight / scaler;

        private static readonly double spaceStep = 10;

        private static readonly double radius = 0.2;

        private static double diameter => radius * 2;

        private static readonly double kernelRadius = diameter * 2;

        private static readonly double solidMass = 2;

        private static readonly double solidDensity = solidMass / Math.PI / Math.Pow(radius, 2);

        private static readonly double fluidMass = 1;

        private static readonly double fluidDensity = fluidMass / Math.PI / Math.Pow(radius, 2);

        private static readonly double relaxScaler = 0.001;

        private static readonly int maxIteration = 3;

        private static readonly double k_small_positive = 0.001;

        private static readonly int particleNum = 10;

        private static readonly Vector gravity = new Vector(0, -9.8d);

        private int index = 0;

        private KernelFunction function = new KernelFunction(kernelRadius);

        private SpaceFinder space = new SpaceFinder(realWidth, realHeight, spaceStep/scaler ,kernelRadius);


        public MainWindow()
        {
            InitializeComponent();
            
            // 手动设置这个高宽
            DrawCanvas.Width = canvasWidth;
            DrawCanvas.Height = canvasHeight;

            // 设置窗体的高宽
            this.Width = canvasWidth + 20;
            this.Height = canvasHeight + 45;

            InitParticles();

            drawTimer.Tick += TimeStep;
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            drawTimer.Start();
        }

        private void InitParticles()
        {
            // 初始化粒子的位置
            double x = 10, y = 10;
            
            // 初始化流体粒子
            for (int i = 0; i < particleNum; i++)
            {
                x = 10;
                for (int j = 0; j < particleNum; j++)
                {
                    var p = new Particle(x, y, scaler, radius)
                    {
                        Fill = Brushes.DarkKhaki,
                        Index = index++,
                        Force = gravity,
                        Mass = fluidMass,
                    };
                    p.Index = index++;
                    allParticles.Add(p);
                    space.ManageParticle(p);
                    x += diameter;

                    DrawCanvas.Children.Add(p);
                }
                y += diameter;
            }
            
            // 初始化边界固体粒子
            InitBoundaries();

            // 计算固体粒子的质量

        }

        private void InitBoundaries()
        {
            // 初始化边界
            // 加入边界粒子
            double x = 0;
            double y = radius;

            // Bottom
            for (int i = 0; i < 2; i++)
            {
                x = diameter * 2.5;
                for (int j = 0; j < (realWidth - diameter * 4) / diameter; j++)
                {
                    var p = new Particle(x, y, scaler, radius)
                    {
                        Fill = Brushes.Black,
                        IsSolid = true,
                        Index = index++,
                        Mass = solidMass
                    };

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y += diameter;
            }

            // Top
            y = realHeight - radius;
            for (int i = 0; i < 2; i++)
            {
                x = diameter * 2.5;
                for (int j = 0; j < (realWidth - diameter * 4) / diameter; j++)
                {
                    var p = new Particle(x, y, scaler, radius)
                    {
                        Fill = Brushes.Black,
                        IsSolid = true,
                        Index = index++,
                        Mass = solidMass
                    };

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y -= diameter;
            }

            // Left
            y = radius;
            for (int i = 0; i < 2; i++)
            {
                x = radius;
                for (int j = 0; j < realHeight / diameter; j++)
                {
                    var p = new Particle(y, x, scaler, radius)
                    {
                        Fill = Brushes.Black,
                        IsSolid = true,
                        Index = index++,
                        Mass = solidMass
                    };

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);
                    x += diameter;
                }
                y += diameter;
            }

            // Right
            y = realWidth - radius;
            for (int i = 0; i < 2; i++)
            {
                x = radius;
                for (int j = 0; j < realHeight / diameter; j++)
                {
                    var p = new Particle(y, x, scaler, radius)
                    {
                        Fill = Brushes.Black,
                        IsSolid = true,
                        Index = index++,
                        Mass = solidMass
                    };

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y -= diameter;
            }
        }

        private double GetTheMaxTimeStep(double minTime, double maxTime)
        {
            // 这里使用的CFL方法
            if (maxSpeed.Length <= 0)
            {
                return maxTime;
            }

            double timeStep = diameter * 5 / maxSpeed.Length;
            timeStep = Math.Min(timeStep, maxTime);
            timeStep = Math.Max(timeStep, minTime);

            return timeStep;
        }

        private void Solver()
        {
            foreach (var particle in allParticles)
            {
                // 先计算密度约束
                double rol_i = 0;
                foreach (var particleNeighbor in particle.Neighbor)
                {
                    var value = function.Poly6(particle.NextPosition - particleNeighbor.NextPosition, kernelRadius);

                    rol_i += particle.Mass * value;
                }

                var c_i = rol_i / fluidDensity - 1;

                if (c_i > 0)
                {
                    Vector grad_i = new Vector();
                    double sum_grad_j = 0;

                    foreach (var neiborParticle in particle.Neighbor)
                    {
                        Vector grad_j = neiborParticle.Mass / fluidDensity * function.SpikyGrad(
                            particle.NextPosition - neiborParticle.NextPosition, kernelRadius);

                        grad_i += grad_j;
                        sum_grad_j += Math.Pow((-grad_j).Length, 2);
                    }

                    sum_grad_j += Math.Pow(grad_i.Length, 2);
                    particle.LambdaMultiplier = -c_i / (sum_grad_j + relaxScaler);
                }
                else
                {
                    particle.LambdaMultiplier = 0;
                }
            }

            foreach (var particle in fluidParticle)
            {
                if(particle.LambdaMultiplier == 0)
                    continue;

                Vector det_p = new Vector();

                foreach (var particle1 in particle.Neighbor)
                {
                    double scorr = -k_small_positive * Math.Pow(
                        function.Spiky(particle.NextPosition - particle1.NextPosition, kernelRadius)
                        / function.Spiky(0.2 * kernelRadius, kernelRadius), 4);

                    det_p += (particle.LambdaMultiplier + particle1.LambdaMultiplier + scorr) *
                             function.SpikyGrad(particle.NextPosition - particle1.NextPosition, kernelRadius);
                }

                particle.OffsetPos = det_p / fluidDensity;
            }

            foreach (var particle in fluidParticle)
            {
                particle.NextPosition += particle.OffsetPos;
                particle.OffsetPos = new Vector();
            }

        }

        public void TimeStep(object sender, EventArgs e)
        {
            drawTimer.Stop();

            double timeStep = GetTheMaxTimeStep(0.16, 0.016);

            foreach (var particle in allParticles)
            {
                particle.PredictPosition(timeStep);
                if (particle.IsSolid)
                {
                    continue;
                }

                if (particle.Velocity.Length > maxSpeed.Length)
                {
                    maxSpeed = particle.Velocity;
                }
            }

            space.FindAllNeighbor();

            for (int i = 0; i < maxIteration; i++)
            {
               Solver(); 
            }

            foreach (var particle in fluidParticle)
            {
                particle.Velocity = (particle.NextPosition - particle.Position) / timeStep;
                
                // 粘度
                Vector vij;
                Vector sum = new Vector();
                foreach (var particle1 in particle.Neighbor)
                {
                    vij = particle1.Velocity - particle.Velocity;
                    vij *= function.Spiky(particle.NextPosition - particle1.NextPosition, kernelRadius) * 0.01;
                    sum += vij;
                }
                particle.Velocity += sum;
            }

            foreach (var particle in fluidParticle)
            {
                particle.SetPosition(particle.NextPosition);

                // 对空间中间的粒子进行更新
                space.UpdatePosition(particle);
            }

            drawTimer.Start();
        }

    }
}
