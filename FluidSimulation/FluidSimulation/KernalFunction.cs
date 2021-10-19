using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation
{
    public class KernelFunction
    {
        public static double Poly6Kernel(Vector radius, double h)
        {
            double length = radius.Length;
            double q = length / h;
            double h3 = Math.Pow(h, 3);

            if (length <= h)
            {
                return 315 / (64 * Math.PI * h3) * Math.Pow((1 - Math.Pow(q, 2)), 3);
            }
            {
                return 0;
            }
        }

        public static double Poly6KernelGrad(Vector radius, double h)
        {
            double length = radius.Length;
            double q2 = Math.Pow(length / h, 2);
            double h5 = Math.Pow(h, 5);

            if (length <= h)
            {
                return -945 * length / (32 * Math.PI * h5) * Math.Pow((1 - q2), 2);
            }
            {
                return 0;
            }
        }

        public static double SpikyKernelGrad(Vector radius, double h)
        {
            double length = radius.Length;
            double h4 = Math.Pow(h, 4);

            if (length<= h)
            {
                return -45 / (Math.PI * h4) * Math.Pow(1 - length / h, 2);
            }
            {
                return 0;
            }
        }
    }
}
