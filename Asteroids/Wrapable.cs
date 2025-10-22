using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Wrapable : Entity
    {
        public static List<Wrapable> Wrapables = new List<Wrapable>();
        internal static RectangleF Bounds = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// Overload for SetBounds(float, float) using Size
        /// </summary>
        /// <param name="s">Size of the screen</param>
        public static void SetBounds(Size s) => SetBounds(s.Width, s.Height);

        /// <summary>
        /// Set the bounds of the screen for wrapping
        /// </summary>
        /// <param name="w">Width of the screen</param>
        /// <param name="h">Height of the screen</param>
        public static void SetBounds(float w, float h)
        {
            if (w <= 0 || h <= 0) throw new Exception("Wrapable bounds must be greater than zero");
            Bounds.Width = w;
            Bounds.Height = h;

            if (Global.DEBUG)
            {
                Bounds.Width /= 2f;
                Bounds.Height /= 2f;
                Bounds.X = Bounds.Width / 2f;
                Bounds.Y = Bounds.Height / 2f;
            }
        }

        /// <summary>
        /// Constructor for the Wrapable class
        /// </summary>
        /// <param name="startPosition">The starting position of the wrapable object</param>
        public Wrapable(Vector2 startPosition) : base(startPosition)
        {
            if (Bounds.Width == 0) throw new Exception("Wrapable Width not set");
            else if (Bounds.Height == 0) throw new Exception("Wrapable Bounds.Height not set");
            else
            {
                Wrapables.Add(this);
            }
        }

        /// <summary>
        /// Wrap the position of the object around the screen edges
        /// </summary>
        public void WrapPosition()
        {
            Vector2 pos = base.position;
            // vertical wrapping
            while (pos.X < 0)
            {
                pos.X += Bounds.Width;
            }
            while (pos.X > Bounds.Width)
            {
                pos.X -= Bounds.Width;
            }

            // horizontal wrapping
            while (pos.Y < 0)
            {
                pos.Y += Bounds.Height;
            }
            while (pos.Y > Bounds.Height)
            {
                pos.Y -= Bounds.Height;
            }

            base.position = pos;
        }

        /// <summary>
        /// Actual drawing method that handles wrapping
        /// </summary>
        /// <param name="g">Graphics object to be drawn to</param>
        public void Draw(Graphics g)
        {
            Vector2[] positions = GetPositions();

            // Draw in the 4 directions
            Draw(g, positions[0] + new Vector2(Bounds.X, Bounds.Y) );
            Draw(g, positions[1] + new Vector2(Bounds.X, Bounds.Y) );
            Draw(g, positions[2] + new Vector2(Bounds.X, Bounds.Y) );
            Draw(g, positions[3] + new Vector2(Bounds.X, Bounds.Y) );

            if (Global.DEBUG)
            {
                g.DrawEllipse(Pens.Red, 
                    position.X + Bounds.X - base.radius, 
                    position.Y + Bounds.Y - base.radius, 
                    base.radius * 2, 
                    base.radius * 2);

                Vector2 velocity = Global.Normalize(this.velocity);
                g.DrawLine(Pens.Green, 
                    Bounds.X + position.X,
                    Bounds.Y + position.Y, 
                    Bounds.X + position.X + velocity.X * radius * Global.DEBUG_DIRECTION_LINE_LENGTH,
                    Bounds.Y + position.Y + velocity.Y * radius * Global.DEBUG_DIRECTION_LINE_LENGTH);

                g.DrawRectangle(Pens.Blue, Bounds);
            }
        }

        /// <summary>
        /// Generic Draw method to be overridden by subclasses
        /// </summary>
        /// <param name="g">Graphics object to be drawn to</param>
        /// <param name="Position">Position to be drawn to</param>
        public virtual void Draw(Graphics g, Vector2 Position)
        {
            throw new NotImplementedException($"Update method not implemented in {this.GetType()}");
        }

        /// <summary>
        /// Gets the 4 positions of the ojbect for wrapping
        /// </summary>
        /// <returns>An array length 4 of Vector2</returns>
        public Vector2[] GetPositions()
        {
            // Get the quadrant signs
            Vector2 Quadrant = new Vector2(
                float.Sign(Bounds.Width / 2 - position.X),
                float.Sign(Bounds.Height / 2 - position.Y)
                );

            return
            [
                position,
                position + new Vector2(Quadrant.X * Bounds.Width, 0),
                position + new Vector2(0, Quadrant.Y * Bounds.Height),
                position + new Vector2(Quadrant.X * Bounds.Width, Quadrant.Y * Bounds.Height)
            ];
        }

        public Vector2 GetClosest(Vector2 Position)
        {
            Vector2[] positions = GetPositions();
            Array.Sort(positions, (a, b) =>
            {
                float distA = Vector2.DistanceSquared(a, Position);
                float distB = Vector2.DistanceSquared(b, Position);
                return distA.CompareTo(distB);
            });
            return positions[0];
        }

        public override void Remove()
        {
            base.Remove();
            Wrapables.Remove(this);
        }
    }
}
