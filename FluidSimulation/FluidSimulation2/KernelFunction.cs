using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation2
{
    public class KernelFunction
    {
        private double kernelRadius;

        public double KernelRadius => this.kernelRadius;

        public KernelFunction(double kernelRadius)
        {
            this.kernelRadius = kernelRadius;
        }
        public double Poly6(Vector radius, double h)
        {
            double length = radius.Length;
            return Poly6(length, h);
        }

        public double Poly6(double radius, double h)
        {

            double length = radius;
            double r2 = Math.Pow(length, 2);
            double h2 = Math.Pow(h, 2);
            double h9 = Math.Pow(h, 9);

            if (length <= h)
            {
                return 315 / (64 * Math.PI * h9) * Math.Pow((h2 - r2), 3);
            }
            {
                return 0;
            }
        }

        public double Poly6Grad(Vector radius, double h)
        {
            double length = radius.Length;
            double q2 = Math.Pow(length / h, 2);
            double h5 = Math.Pow(h, 5);

            if (length <= h)
            {
                return -945 * length / (32 * Math.PI * h5) * Math.Pow((1 - q2), 2);
            }
            {
                return 0;// var  = length * v1 * Math.Cos(Vector.AngleBetween(offset, v1))) ;
            }
        }

        public double Spiky(Vector radius, double h)
        {
            double length = radius.Length;
            return Spiky(length, h);
        }

        public double Spiky(double radius, double h)
        {
            double length = radius;
            double h6 = Math.Pow(h, 6);
            if (length <= h)
            {
                return 15 / (Math.PI * h6) * Math.Pow(h - length, 3);
            }
            {
                return 0;
            }
        }

        public Vector SpikyGrad(Vector radius, double h)
        {
            double length = radius.Length;
            double h4 = Math.Pow(h, 4);

            if (0 < length && length <= h)
            {
                return -45 / (Math.PI * h4) * Math.Pow(1 - length / h, 2) * (radius / length);
            }
            {
                return new Vector(0, 0);
            }
        }
    }
}
