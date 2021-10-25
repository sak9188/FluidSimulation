using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidSimulation2
{
    public class SpaceFinder
    {
        private double width;
        private double height;
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
        }

        public void ManageParticle(Particle particle)
        {
            particles.Add(particle);
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

                                // TODO 这里可能存在一个埃普西隆的误差
                                if ((particle1.NextPosition - particle1.NextPosition).Length <= this.radius)
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
    }
}
