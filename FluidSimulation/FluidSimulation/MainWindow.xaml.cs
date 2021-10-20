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
            timer.Interval = new TimeSpan(0,0,0,0,Convert.ToInt32(timeInterval * 1000));
            timer.Start();
        }

        public List<Particle> allParticle = new List<Particle>();

        // 长宽都是1000, 1000, 单个格子的长度为10, 总计 100*100
        public const int SpaceStep = 10;
        public Dictionary<int, HashSet<Particle>> SpaceDict = new Dictionary<int, HashSet<Particle>>();

        public readonly Vector gravity = new Vector(0, -9.8d);
        private void InitDrawCanvas()
        {
            //DrawCanvas.VerticalAlignment = VerticalAlignment.Top;
            //DrawCanvas.HorizontalAlignment = HorizontalAlignment.Left;

            // 初始化粒子
            uint particleNum = 50;

            double x = 0, y = 500;
            double radius = 2.5;
            double diameter = radius * 2;

            for (int i = 0; i < particleNum; i++)
            {
                x = 0;
                for (int j = 0; j < particleNum; j++)
                {
                    var e = new Particle(x, y, radius)
                    {
                        Fill = Brushes.DarkKhaki
                    };

                    allParticle.Add(e);

                    e.Force = gravity; 
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
        private readonly double timeInterval = 0.016;

        private readonly double timeStep = 0.1;
        // 目标帧数
        private readonly int frames = 1000;

        private int curFrames = 0;

        private Rect wallRect = new Rect(0, 0, 1000, 1000);

        private readonly int MaxIteration = 5;

        private double restDensity = 1;

        private double relaxScaler = 1;

        private double k_small_positive = -0.01;

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
                    foreach (var particleNeighbor in particle.NeighborParticles)
                    {
                        rol_i += KernelFunction.Poly6Kernel(particleNeighbor.Position - particle.Position,
                            particle.Radius * 2);
                    }
                    
                    var c_i = rol_i - 1;

                    double pkc = 0;
                    
                    // 这里存储着有关lambda_j所需的模长
                    List<double> list_lambda_j = new List<double>();
                    List<double> list_real_j = new List<double>();
                    List<Vector> list_grad_j = new List<Vector>();
                    foreach (var particleNeighborParticle in particle.NeighborParticles)
                    {
                        var offset = particle.NextPosition - particleNeighborParticle.NextPosition;
                        var kernelValue = KernelFunction.SpikyKernel(offset, particle.Diameter);
                        var kernelGradValue = KernelFunction.SpikyKernelGrad(offset,particle.Radius * 2);
;
                        var c_pk = Math.Pow(kernelGradValue.Length, 2);
                        pkc += c_pk; 
                        list_lambda_j.Add(-c_i / (c_pk + relaxScaler));
                        list_real_j.Add(kernelValue);
                        list_grad_j.Add(kernelGradValue);
                    }

                    var lambda_i = -c_i / (pkc + relaxScaler);

                    // 计算det_p
                    Vector det_p = new Vector();

                    for (int j = 0; j < list_lambda_j.Count; j++)
                    {
                        var lambda_j = list_lambda_j[j];
                        var grad_j = list_grad_j[j];

                        var real_j = list_real_j[j];
                        var read_q = KernelFunction.SpikyKernel(0.2 * 5, 5);

                        var scorr = -k_small_positive * Math.Pow(real_j / read_q, 4);

                        det_p = (lambda_i + lambda_j + scorr) * grad_j;
                    }

                    // 计算碰撞
                    // 这里我设置以墙为碰撞体
                    var constraintPos = particle.NextPosition + det_p;

                    if (constraintPos.X - particle.Radius < 0)
                        constraintPos.X = particle.Radius;
                    else if (constraintPos.X + particle.Radius > 1000)
                        constraintPos.X = 1000 - particle.Radius;

                    if (constraintPos.Y - particle.Radius < 0)
                        constraintPos.Y = particle.Radius;
                    else if (constraintPos.Y + particle.Radius > 1000)
                        constraintPos.Y = 1000 - particle.Radius;

                    // 更新位置
                    particle.NextPosition = constraintPos;
                }
            }

            foreach (var particle in allParticle)
            {
                // 更新速度
                particle.Velocity = (particle.NextPosition - particle.Position) / timeStep;
                // 加入旋度，粘度
                if (double.IsNaN(particle.NextPosition.X) || double.IsNaN(particle.NextPosition.Y))
                {
                    int test_point = 0;
                }
                // 更新位置
                var oldKey = GetSpaceDictKey(particle.Position);

                if(oldKey == 2424)
                {
                    int test_poiint = 0;
                }
                var newKey = GetSpaceDictKey(particle.NextPosition);
                particle.SetPosition(particle.NextPosition);

                if(oldKey != newKey)
                {
                    // 设置空间映射
                    var treeSet = SpaceDict[oldKey];
                    treeSet.Remove(particle); 
                    
                    HashSet<Particle> set;
                    if (SpaceDict.TryGetValue(newKey, out set))
                    {
                        set.Add(particle);
                    }
                    {
                        SpaceDict[newKey] = new HashSet<Particle>() { particle };
                    }
                }
            }
        }

        public static int GetSpaceDictKey(Vector vector)
        {
            var xPos = Convert.ToInt32(vector.X / SpaceStep);
            var yPos = Convert.ToInt32(vector.Y / SpaceStep);

            return xPos + yPos * 100;
        }
    }
}
