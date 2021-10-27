using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation2
{
    public class SpaceFinder
    {
        private double width;
        private double height;
        private int rowNum;
        private double step;
        private double radius;

        private List<Particle> particles = new List<Particle>();

        private Dictionary<int, HashSet<Particle>> spaceDict = new Dictionary<int, HashSet<Particle>>();

        public SpaceFinder(double width, double height, double singleStep, double kernelRadius)
        {
            this.width = width;
            this.height = height;
            this.step = singleStep;
            this.radius = kernelRadius;
            this.rowNum = Convert.ToInt32(width / singleStep);
        }

        private int GetKey(Vector position)
        {
            var xPos = Convert.ToInt32(position.X / this.step);
            var yPos = Convert.ToInt32(position.Y / this.step);

            return xPos + yPos * rowNum;
        }

        public void ManageParticle(Particle particle)
        {
            particles.Add(particle);

            var key = GetKey(particle.Position);
            HashSet<Particle> set;
            if(spaceDict.TryGetValue(key, out set))
            {
                set.Add(particle);
            }
            else
            {
                spaceDict[key] = new HashSet<Particle>() {particle};
            }
        }

        public void FindAllNeighbor()
        {
            foreach (var particle in particles)
            {
                var xGridPos = Convert.ToInt32(particle.NextPosition.X / step);
                var yGridPos = Convert.ToInt32(particle.NextPosition.Y / step);

                HashSet<Particle> adjoin = new HashSet<Particle>();
                for (int i = -1 ; i < 2; i++)
                {
                    for (int j = -1 ; j < 2; j++)
                    {
                        HashSet<Particle> hashSet;
                        if(spaceDict.TryGetValue(xGridPos + j + (yGridPos + i) * 8, out hashSet))
                        {
                            foreach (var particle1 in hashSet)
                            {
                                if(particle1 == particle)
                                    continue;

                                // 这里算出来的向量的长度会有误差，所以这里需要计算两者之间的差
                                if (Math.Abs((particle1.NextPosition - particle1.NextPosition).Length - this.radius) <= 0.01)
                                {
                                    adjoin.Add(particle1);
                                }
                            }
                        }
                    } 
                }

                particle.Neighbor = adjoin;
            }
        }

        public void UpdatePosition(Particle updateParticle)
        {
            // 更新所有的位置信息

        }
    }
}
