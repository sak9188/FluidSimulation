using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FluidSimulation
{
    public class Particle : Shape
    {
        private Rect _rect = Rect.Empty;
        static Particle() => Shape.StretchProperty.OverrideMetadata(typeof (Particle), (PropertyMetadata) new FrameworkPropertyMetadata((object) Stretch.Fill));

        public override Geometry RenderedGeometry => this.DefiningGeometry;

        public override Transform GeometryTransform => Transform.Identity;

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(Diameter, Diameter);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize.Width = radius;
            finalSize.Height = radius;
            return finalSize;
        }

        protected override Geometry DefiningGeometry =>
            this._rect.IsEmpty ? Geometry.Empty : (Geometry) new EllipseGeometry(this._rect);

        private Pen _pen;

        protected Pen GetPen()
        {
            if (this._pen == null)
            {
                double num = Math.Abs(this.StrokeThickness);
                this._pen = new Pen();
                this._pen.Thickness = num;
                this._pen.Brush = this.Stroke;
                this._pen.StartLineCap = this.StrokeStartLineCap;
                this._pen.EndLineCap = this.StrokeEndLineCap;
                this._pen.DashCap = this.StrokeDashCap;
                this._pen.LineJoin = this.StrokeLineJoin;
                this._pen.MiterLimit = this.StrokeMiterLimit;
            }

            return this._pen;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this._rect.IsEmpty)
                return;
            Pen pen = this.GetPen();
            drawingContext.DrawGeometry(this.Fill, pen, (Geometry) new EllipseGeometry(this._rect));
        }

        private double radius;

        public double Diameter => radius * 2;

        public Particle(double x, double y, double radius=2.5)
        {
            position.X = x;
            position.Y = y;
            this.radius = radius;
            this._rect = new Rect(new Size(this.Diameter, this.Diameter));
        }

        public double Radius
        {
            get
            {
                return radius;
            }

            set
            {
                radius = value;
                _rect.X = radius;
                _rect.Y = radius;
            }
        }

        private Vector position = new Vector(0, 0);

        public Vector Position => this.position;

        public void SetPosition(double x, double y)
        {
            position.X = x;
            position.Y = y;

            // this._rect
        }

        private Vector velocity = new Vector(0, 0);

        public Vector Velocity
        {
            get
            {
                return velocity;
            }

            set
            {
                velocity = value;
            }
        }

        private Vector force = new Vector(0, 0);

        public Vector Force
        {
            get
            {
                return force;
            }

            set
            {
                force = value;
            }
        }
    }
}
