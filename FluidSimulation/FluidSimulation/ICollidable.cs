using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation
{
    public interface ICollidable
    {
        Rect GetCollideShape();

        // 是否可以移动
        bool IsMovable();
    }
}
