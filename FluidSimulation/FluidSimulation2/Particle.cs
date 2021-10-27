using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FluidSimulation2
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
            drawingContext.DrawText(
               new FormattedText(this.index.ToString(),
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Microsoft Yahei"),
                  this.Diameter, Brushes.Black),
                  new Point(_rect.X, _rect.Y));

        }

        private int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        private HashSet<Particle> neighbor = new HashSet<Particle>();

        public HashSet<Particle> Neighbor
        {
            get => neighbor;
            set => neighbor = value;
        }

        private double scaler = 1;

        public double Scaler
        {
            get
            {
                return scaler;
            }
            set
            {
                scaler = value;
            }
        }

        private double radius;

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

        public double Diameter => radius * 2;

        private Vector position = new Vector(0, 0);

        public Vector Position => this.position;

        private bool isSolid = false;

        public bool IsSolid
        {
            get
            {
                return isSolid;
            }

            set
            {
                isSolid = value;
            }
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

        private double lambdaMultipiler = 0;

        public double LambdaMultiplier
        {
            get => lambdaMultipiler;
            set => lambdaMultipiler = value;
        }

        private Vector offsetPos = new Vector();

        public Vector OffsetPos
        {
            get => offsetPos;
            set => offsetPos = value;
        }

        private double mass = 1;

        public double Mass
        {
            get => mass;

            set => mass = value;
        }

        public double Density
        {
            get => Mass / (Math.PI * Math.Pow(radius, 2));
        }

        private Vector nextPosition;

        public Vector NextPosition
        {
            get => nextPosition;
            set => nextPosition = value;
        }

        public Particle(double x, double y, int scaler, double radius=2.5)
        {
            position.X = x;
            position.Y = y;
            this.radius = radius;
            this.scaler = scaler;
            this._rect = new Rect(new Size(this.Diameter * scaler, this.Diameter * scaler));
            SetPosition(x, y);
        }

        public void SetPosition(double x, double y)
        {
            position.X = x;
            position.Y = y;
            _SetPosition(position.X, position.Y);
        }

        public void SetPosition(Vector vector)
        {
            position = vector;
            _SetPosition(position.X, position.Y);
        }

        private void _SetPosition(double x, double y)
        {
            Canvas.SetLeft(this, (x - this.radius) * scaler);
            Canvas.SetBottom(this, (y + this.radius) * scaler);
        }

        public void PredictPosition(double time)
        {
            // 因为目前只有重力, 后面需要有其他力对速度进行修正
            velocity += force * time;
            var posOffset = velocity * time;
            NextPosition = this.position + posOffset;
        }
    }
}
