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

        private static double radius = 0.3;
        private static double diameter => radius * 2;

        private static double kernelRadius = diameter * 2;

        private static double solidMass = 2;

        private static double solidDensity = solidMass / Math.PI / Math.Pow(radius, 2);

        private static double fluidMass = 1;

        private static double fluidDensity = fluidMass / Math.PI / Math.Pow(radius, 2);

        private static double relaxScaler = 0.001;

        private readonly int maxIteration = 5;

        private readonly double k_small_positive = 0.001;

        private KernelFunction function = new KernelFunction(kernelRadius);

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
            return 0;
        }

        private void Solver(double timeStep)
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
        }

        public void TimeStep(object sender, EventArgs e)
        {
            drawTimer.Stop();

            double timeStep = GetTheMaxTimeStep(0.016, 0.0016);

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

            for (int i = 0; i < maxIteration; i++)
            {
               Solver(); 
            }

            foreach (var particle in allParticles)
            {
                // 更新速度
                particle.Velocity = (particle.NextPosition - particle.Position) / timeStep;
                // 加入旋度
                // 加入粘度
            }

            drawTimer.Start();
        }

    }
}
