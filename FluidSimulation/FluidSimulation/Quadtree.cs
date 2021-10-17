using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FluidSimulation
{
    // 四叉树节点
    public class Quadnode<T> where T: ICollidable
    {
        // 这里不适合使用四叉树，因为这个是游戏中静态物体的管理
        public enum EOrientation
        {
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom
        }

        // 只有4个方向有子节点
        public Quadnode<T>[] Children = new Quadnode<T>[4];

        public bool IsRoot { get; set; }

        // 当前节点单元格的大小
        private Vector size;
        
        // 最小单元格的大小
        private double resolution;
        
        private T dataObject;

        private Vector position;

        // 这个八叉树的大小
        public Quadnode(Vector position, Vector size, double resolution)
        {
            this.position = position;
            this.size = size;
            this.resolution = resolution;
            this.IsRoot = true;
        }

        // 这个方法是构建子节点的方法
        public Quadnode(Quadnode<T> parent)
        {
            this.IsRoot = false;
            this.size = parent.size / 2;
        }

        public void AddChild(T dataObject)
        {
            // 一个碰撞体, 获得位置信息
            var collide = dataObject.GetCollideShape();
            
        }

        public EOrientation GetChildOrientation(T dataCollidable)
        {
            // 和存储的对象处于统一坐标系统下
            var colide = dataCollidable.GetCollideShape();
            
            // 四叉树一共有
            return EOrientation.LeftBottom;
        }

}
    // 四叉树， 用空间换时间
    public class Quadtree
    {
        
    }
}
