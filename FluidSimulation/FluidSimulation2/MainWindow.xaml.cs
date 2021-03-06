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

        private static readonly double volume = Math.PI * Math.Pow(radius, 2);

        private static readonly double solidDensity = 90;

        private static readonly double solidMass = volume * solidDensity;

        private static readonly double fluidDensity = 10;

        private static readonly double fluidMass = volume * fluidDensity;

        private static readonly double relaxScaler = 0.001;

        private static readonly int maxIteration = 5;

        private static readonly double k_small_positive = 0.001;

        private static readonly int particleNum = 50;

        private static readonly Vector gravity = new Vector(0, -9.8d);

        private int index = 0;

        private KernelFunction function = new KernelFunction(kernelRadius);

        private SpaceFinder space = new SpaceFinder(realWidth, realHeight, spaceStep/scaler ,kernelRadius);


        public MainWindow()
        {
            InitializeComponent();
            
            // ????????????????????????
            DrawCanvas.Width = canvasWidth;
            DrawCanvas.Height = canvasHeight;

            // ?????????????????????
            this.Width = canvasWidth + 20;
            this.Height = canvasHeight + 45;

            InitParticles();


        }

        private void InitParticles()
        {
            // ????????????????????????
            double x = 10, y = 10;
            
            // ?????????????????????
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
                    fluidParticle.Add(p);
                    space.ManageParticle(p);
                    x += diameter;

                    DrawCanvas.Children.Add(p);
                }
                y += diameter;
            }
            
            // ???????????????????????????
            InitBoundaries();

            // ????????????????????????????????????????????????????????????????????????
            space.FindAllNeighbor();

            // ???????????????????????????
            foreach (var particle in solidParticle)
            {
                var mass = fluidDensity * solidMass;
                var sum_mass = function.Poly6(0, kernelRadius);
                foreach (var particle1 in particle.Neighbor)
                {
                    if(!particle1.IsSolid)
                        continue;

                    sum_mass += solidMass * function.Poly6(particle.Position - particle1.Position, kernelRadius);
                }

                // ??????????????????
                particle.Mass = mass / sum_mass;
            }

        }

        private void InitBoundaries()
        {
            // ???????????????
            // ??????????????????
            double x = 0;
            double y = radius;

            // Bottom
            for (int i = 0; i < 3; i++)
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

                    p.NextPosition = p.Position;

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y += diameter * 0.33;
            }

            // Top
            y = realHeight - radius;
            for (int i = 0; i < 3; i++)
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

                    p.NextPosition = p.Position;

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y -= diameter * 0.33;
            }

            // Left
            y = radius;
            for (int i = 0; i < 3; i++)
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

                    p.NextPosition = p.Position;

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);
                    x += diameter;
                }
                y += diameter * 0.33;
            }

            // Right
            y = realWidth - radius;
            for (int i = 0; i < 3; i++)
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

                    p.NextPosition = p.Position;

                    allParticles.Add(p);
                    solidParticle.Add(p);
                    space.ManageParticle(p);
                    DrawCanvas.Children.Add(p);

                    x += diameter;
                }
                y -= diameter * 0.33;
            }
        }

        private double GetTheMaxTimeStep(double minTime, double maxTime)
        {
            // ???????????????CFL??????
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
                // ?????????????????????
                double rol_i = 0;
                foreach (var particleNeighbor in particle.Neighbor)
                {
                    if (!particle.IsSolid)
                    {
                        int t = 0;
                    }
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
                        / function.Spiky(0.25 * kernelRadius, kernelRadius), 4);

                    det_p += (particle.LambdaMultiplier + particle1.LambdaMultiplier + scorr) *
                             function.SpikyGrad(particle.NextPosition - particle1.NextPosition, kernelRadius);
                }

                particle.OffsetPos = det_p / fluidDensity;
            }

            foreach (var particle in fluidParticle)
            {
                if(particle.OffsetPos.Length <= 0)
                    continue;

                particle.NextPosition += particle.OffsetPos;
                particle.OffsetPos = new Vector();
            }

        }

        public void TimeStep(object sender, EventArgs e)
        {
            drawTimer.Stop();

            double timeStep = GetTheMaxTimeStep(0.016, 0.0005);

            foreach (var particle in fluidParticle)
            {
                particle.PredictPosition(timeStep);
            }

            space.FindAllNeighbor();

            for (int i = 0; i < maxIteration; i++)
            {
               Solver();
            }

            maxSpeed = new Vector();

            foreach (var particle in fluidParticle)
            {
                particle.Velocity = (particle.NextPosition - particle.Position) / timeStep;
                
                // ??????
                Vector vij;
                Vector sum = new Vector();
                foreach (var particle1 in particle.Neighbor)
                {
                    vij = particle1.Velocity - particle.Velocity;
                    vij *= function.Spiky(particle.NextPosition - particle1.NextPosition, kernelRadius) * 0.01;
                    sum += vij;
                }
                particle.Velocity += sum;

                if (particle.Velocity.Length > maxSpeed.Length)
                {
                    maxSpeed = particle.Velocity;
                }
            }

            foreach (var particle in fluidParticle)
            {
                // ????????????????????????????????????
                space.UpdatePosition(particle);

                particle.SetPosition(particle.NextPosition);
            }

            drawTimer.Start();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.Visibility = Visibility.Hidden;
            
            drawTimer.Tick += TimeStep;
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            drawTimer.Start();
        }
    }
}
