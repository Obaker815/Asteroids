using System.Numerics;

namespace Asteroids
{
    internal class Wrapable : Entity
    {
        public static List<Wrapable> Wrapables = new List<Wrapable>();

        private static float width = 0;
        private static float height = 0;

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
            width = w;
            height = h;
        }

        /// <summary>
        /// Constructor for the Wrapable class
        /// </summary>
        /// <param name="startPosition">The starting position of the wrapable object</param>
        public Wrapable(Vector2 startPosition) : base(startPosition)
        {
            if (width == 0) throw new Exception("Wrapable width not set");
            if (height == 0) throw new Exception("Wrapable height not set");
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
                pos.X += width;
            }
            while (pos.X > width)
            {
                pos.X -= width;
            }

            // horizontal wrapping
            while (pos.Y < 0)
            {
                pos.Y += height;
            }
            while (pos.Y > height)
            {
                pos.Y -= height;
            }

            base.position = pos;
        }
    }
}
