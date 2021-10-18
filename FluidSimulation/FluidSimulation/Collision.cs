using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation
{
    public class Collision
    {

        public void CollisionCalculate(IList<ICollidable> list)
        {
            // 这里需要一个八叉树算法 TODO

            foreach (var collidable in list)
            {
                // ON^2
                foreach (var collidableNext in list)
                {
                    // 这里其实做了简化, 因为目前只有一个约束
                    // 那就是位置不能穿透

                    // 判断这两个是否发生了碰撞
                    if(IsCollide(collidable, collidableNext))
                    {
                        // 如果在当前速度下

                    }
                }    
            }
        }

        public bool IsCollide(ICollidable collide, ICollidable collideNext)
        {
            if (collide == collideNext)
            {
                return false;
            }

            var collideShape = collide.GetCollideShape();

            var collideNextShape = collideNext.GetCollideShape();

            // 获得两者的形状以后，这里做简化处理， 因为粒子之间都是圆形，且都有相同半径
            Vector substruction = new Vector(collideShape.X - collideNextShape.X, collideShape.Y - collideNextShape.Y);

            return substruction.Length < collideNextShape.Width;
        }
    }
}
