using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            timer.Start();
        }

        private void InitDrawCanvas()
        {
            //DrawCanvas.VerticalAlignment = VerticalAlignment.Top;
            //DrawCanvas.HorizontalAlignment = HorizontalAlignment.Left;

            // 初始化粒子
            uint particleNum = 10;

            double x = 0, y = 100;
            int index = 0;

            for (int i = 0; i < particleNum; i++)
            {
                x = 100;
                for (int j = 0; j < particleNum; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.DarkKhaki
                    };
                    e.index = index;
                    index++;
                    allParticle.Add(e);
                    fluidParticle.Add(e);
                    e.Force = gravity;
                    e.SetPosition(x, y);
                    // 设置空间映射
                    var xPos = Convert.ToInt32(x / SpaceStep);
                    var yPos = Convert.ToInt32(y / SpaceStep);

                    var key = xPos + yPos * 8;
                    HashSet<Particle> set = null;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    else
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }
                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y += diameter;
            }

            // 加入边界粒子
            y = 0;
            for (int i = 0; i < 2; i++)
            {
                x = diameter * 2;
                for (int j = 0; j < (800 - diameter * 4) / diameter; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.AliceBlue
                    };

                    e.SetPosition(x, y);
                    e.IsSolid = true;
                    e.index = -1;

                    allParticle.Add(e);
                    solidParticle.Add(e);
                    // 设置空间映射
                    var xPos = Convert.ToInt32(x / SpaceStep);
                    var yPos = Convert.ToInt32(y / SpaceStep);

                    var key = xPos + yPos * 8;
                    HashSet<Particle> set = null;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    else
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }
                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y += diameter;
            }

            y = 800;
            for (int i = 0; i < 2; i++)
            {
                x = diameter * 2;
                for (int j = 0; j < (800 - diameter * 4) / diameter; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.Aquamarine
                    };

                    e.SetPosition(x, y);
                    e.IsSolid = true;
                    e.index = -1;

                    allParticle.Add(e);
                    solidParticle.Add(e);

                    // 设置空间映射
                    var xPos = Convert.ToInt32(x / SpaceStep);
                    var yPos = Convert.ToInt32(y / SpaceStep);

                    var key = xPos + yPos * 8;
                    HashSet<Particle> set = null;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    else
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }
                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y -= diameter;
            }

            y = 0;
            for (int i = 0; i < 2; i++)
            {
                x = 0;
                for (int j = 0; j < 800 / diameter; j++)
                {
                    var e = new Particle(y, x, radius)
                    {
                        Fill = Brushes.Black
                    };

                    e.SetPosition(y, x);
                    e.IsSolid = true;
                    e.index = -1;

                    allParticle.Add(e);
                    solidParticle.Add(e);

                    // 设置空间映射
                    var xPos = Convert.ToInt32(y / SpaceStep);
                    var yPos = Convert.ToInt32(x / SpaceStep);

                    var key = xPos + yPos * 8;
                    HashSet<Particle> set = null;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    else
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }
                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y += diameter;
            }

            y = 800 - diameter;
            for (int i = 0; i < 2; i++)
            {
                x = 0;
                for (int j = 0; j < 800 / diameter; j++)
                {
                    var e = new Particle(y, x, radius)
                    {
                        Fill = Brushes.Black
                    };

                    e.SetPosition(y, x);
                    e.IsSolid = true;
                    e.index = -1;

                    allParticle.Add(e);
                    solidParticle.Add(e);
                    // 设置空间映射
                    var xPos = Convert.ToInt32(y / SpaceStep);
                    var yPos = Convert.ToInt32(x / SpaceStep);

                    var key = xPos + yPos * 8;
                    HashSet<Particle> set;
                    if (SpaceDict.TryGetValue(key, out set))
                    {
                        set.Add(e);
                    }
                    else
                    {
                        SpaceDict[key] = new HashSet<Particle>() { e };
                    }
                    DrawCanvas.Children.Add(e);
                    x += diameter;
                }
                y -= diameter;
            }

            foreach (var particle in solidParticle)
            {
                // 找到所有的相邻的粒子
                var xGridPos = Convert.ToInt32(particle.NextPosition.X * scaler / SpaceStep);
                var yGridPos = Convert.ToInt32(particle.NextPosition.Y * scaler / SpaceStep);

                HashSet<Particle> allJoinParticles = new HashSet<Particle>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        HashSet<Particle> hashSet;
                        if (SpaceDict.TryGetValue(xGridPos + j + (yGridPos + i) * 8, out hashSet))
                        {
                            foreach (var particle1 in hashSet)
                            {
                                if (particle1 == particle)
                                    continue;

                                if (!particle1.IsSolid)
                                    continue;

                                if ((particle1.Position - particle.NextPosition).Length <= kernalRadius/scaler)
                                {
                                    allJoinParticles.Add(particle1);
                                }
                            }
                        }
                    }
                }
                particle.NeighborParticles = allJoinParticles;
            }


            // 计算固体粒子
            foreach (var particle in solidParticle)
            {
                var mk = density_0 * 2;
                var sum_ml = KernelFunction.Poly6Kernel(0, kernalRadius);
                foreach (var particleNeighborParticle in particle.NeighborParticles)
                {
                    if (!particleNeighborParticle.IsSolid)
                        continue;

                    sum_ml += 2 * KernelFunction.Poly6Kernel((particle.Position - particleNeighborParticle.Position),
                        kernalRadius / scaler);
                }

                particle.property = mk / sum_ml;
            }

        }

        // 时间步长
        private static double radius = 10;

        private static double diameter = radius * 2;

        private static double kernalRadius = diameter + 1;

        private double scaler = 10;

        private readonly int MaxIteration = 5;

        private static double relaxScaler = 0.000001;

        private double k_small_positive = -0.001;

        private double density_0 = 1 / (Math.PI * Math.Pow(radius, 2));

        static Vector maxSpeed = new Vector(relaxScaler, relaxScaler);

        // 长宽都是1000, 1000, 单个格子的长度为10, 总计 100*100
        public const int SpaceStep = 100;
        
        public Dictionary<int, HashSet<Particle>> SpaceDict = new Dictionary<int, HashSet<Particle>>();

        public readonly Vector gravity = new Vector(0, -9.8d);

        public List<Particle> allParticle = new List<Particle>();
        public List<Particle> fluidParticle = new List<Particle>();
        public List<Particle> solidParticle = new List<Particle>();

        private void TimeStep(object sender, EventArgs e)
        {
            timer.Stop();
            // 求出最大时间步长
            double timeStep = diameter / 10 * 0.4 / maxSpeed.Length;

            timeStep = Math.Min(timeStep, 0.005);
            timeStep = Math.Max(timeStep, 0.0001);

            // 添加重力
            foreach (var particle in allParticle)
            {
                if (particle.IsSolid)
                {
                    particle.PredictPosition(0);
                    continue;
                }
                particle.PredictPosition(timeStep);
                if (particle.Velocity.Length > maxSpeed.Length)
                    maxSpeed = particle.Velocity;
                // particle.SetPosition(particle.Position + particle.NextPosition);// 更新位置
            }

            // 找到所有的临边
            foreach (var particle in allParticle)
            {
                // 找到所有的相邻的粒子
                var xGridPos = Convert.ToInt32(particle.NextPosition.X / SpaceStep);
                var yGridPos = Convert.ToInt32(particle.NextPosition.Y / SpaceStep);

                //Console.WriteLine( "CurParticleIndex:{0}", particle.index);
                HashSet<Particle> allJoinParticles = new HashSet<Particle>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        HashSet<Particle> hashSet;
                        if (SpaceDict.TryGetValue(xGridPos + j + (yGridPos + i) * 8, out hashSet))
                        {
                            foreach (var particle1 in hashSet)
                            {
                                if (particle1 == particle)
                                    continue;
                                
                                // 这里加1是因为存在误差
                                if ((particle1.NextPosition - particle.NextPosition).Length <= kernalRadius+1)
                                {
                                    allJoinParticles.Add(particle1);
                                    //Console.WriteLine("NeibhorParticle:{0}", particle1.index);
                                }
                            }
                        }
                    }
                }
                particle.NeighborParticles = allJoinParticles;
            }

            for (int iter = 0; iter < MaxIteration; iter++)
            {

                // 计算lambda
                foreach (var particle in allParticle)
                {

                    // 密度约束
                    // Ci = rol_i/rol_0 - 1 = 0
                    // rol_i = sum(J, mj, KernalFunction)
                    double rol_i = 0; 
                    //Console.WriteLine( "CurParticleIndex:{0}", particle.index);
                    foreach (var particleNeighbor in particle.NeighborParticles)
                    {
                        //Console.WriteLine("NeibhorParticle:{0}", particleNeighbor.index);
                        var value = KernelFunction.Poly6Kernel((particle.NextPosition - particleNeighbor.NextPosition),
                            kernalRadius/scaler);
                        if (particleNeighbor.IsSolid)
                        {
                            rol_i += particleNeighbor.property * value;
                        }
                        else
                        {
                            rol_i += 1 * value;
                        }
                    }

                    var c_i = rol_i / density_0 - 1;


                    if (c_i > 0)
                    {
                        Vector grad_i = new Vector();
                        double sum_grad_j = 0;
                        foreach (var particleNeighborParticle in particle.NeighborParticles)
                        {
                            Vector grad_j;
                            if (particleNeighborParticle.IsSolid)
                            {
                                grad_j = particleNeighborParticle.property / density_0 * KernelFunction.SpikyKernelGrad(
                                    (particle.NextPosition - particleNeighborParticle.NextPosition), kernalRadius / scaler);
                            }
                            else
                            {
                                grad_j = 1 / density_0 * KernelFunction.SpikyKernelGrad(
                                (particle.NextPosition - particleNeighborParticle.NextPosition), kernalRadius/scaler);
                            }
                            grad_i += grad_j;
                            sum_grad_j += Math.Pow((-grad_j).Length, 2);
                        }

                        sum_grad_j += Math.Pow(grad_i.Length, 2);
                        particle.lambda_multiplyer = -c_i / (sum_grad_j + relaxScaler);
                        if (!particle.IsSolid)
                        {
                            Console.WriteLine("get_lambda_calculator:{0}", particle.index);
                        }
                    }
                    else
                    {
                        particle.lambda_multiplyer = 0;
                    }
                }

                // 计算det_p
                foreach (var particle in fluidParticle)
                {
                    if (particle.lambda_multiplyer == 0)
                        continue;

                    Vector det_p = new Vector();
                    foreach (var particleNeighborParticle in particle.NeighborParticles)
                    {
                        double scorr = -k_small_positive *
                                       Math.Pow(
                                           KernelFunction.SpikyKernel((particle.NextPosition -
                                                                      particleNeighborParticle.NextPosition)/scaler, kernalRadius/scaler)
                                           / KernelFunction.SpikyKernel(0.2 * kernalRadius/scaler, kernalRadius/scaler), 4);

                        det_p += (particle.lambda_multiplyer + particleNeighborParticle.lambda_multiplyer + scorr) *
                            KernelFunction.SpikyKernelGrad(
                                (particle.NextPosition - particleNeighborParticle.NextPosition)/scaler, kernalRadius/scaler);
                    }
                    particle.Delt_p = det_p / density_0;
                }

                foreach (var particle in fluidParticle)
                {
                    particle.NextPosition += particle.Delt_p;
                    particle.Delt_p = new Vector();
                }
            }

            Vector allVelocity = new Vector();
            foreach (var particle in fluidParticle)
            {
                // 更新速度
                particle.Velocity = (particle.NextPosition - particle.Position) / timeStep;
                // 加入旋度
                // 
                // 粘度
                Vector v_ij;
                Vector sum = new Vector();
                foreach (var particleNeighborParticle in particle.NeighborParticles)
                {
                    v_ij = particle.Velocity - particleNeighborParticle.Velocity;
                    v_ij *= KernelFunction.SpikyKernel((particle.NextPosition - particleNeighborParticle.Position) / scaler,
                        kernalRadius / scaler);
                    sum += v_ij;
                }
                particle.Velocity -= sum;
                // allVelocity += particle.Velocity;

                // 更新位置
                var oldKey = GetSpaceDictKey(particle.Position * 10);
                var newKey = GetSpaceDictKey(particle.NextPosition * 10);
                particle.SetPosition(particle.NextPosition);

                if (oldKey != newKey)
                {
                    // 设置空间映射
                    var treeSet = SpaceDict[oldKey];
                    treeSet.Remove(particle);

                    HashSet<Particle> set;
                    if (SpaceDict.TryGetValue(newKey, out set))
                    {
                        set.Add(particle);
                    }
                    else
                    {
                        SpaceDict[newKey] = new HashSet<Particle>() { particle };
                    }

                }
            }

            //Console.WriteLine(allVelocity.Length);
            timer.Start();
        }

        public static int GetSpaceDictKey(Vector vector)
        {
            var xPos = Convert.ToInt32(vector.X / SpaceStep);
            var yPos = Convert.ToInt32(vector.Y / SpaceStep);

            return xPos + yPos * 8;
        }
    }
}
