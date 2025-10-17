using System.Numerics;

namespace Asteroids
{
    internal class Wrapable : Entity
    {
        private static float width = 0;
        private static float height = 0;



        /// <summary>
        /// Constructor for the Wrapable class
        /// </summary>
        /// <param name="startPosition">The starting position of the wrapable object</param>
        public Wrapable(Vector2 startPosition) : base(startPosition)
        {
            if (width == 0) throw new Exception("Wrapable width not set");
            if (height == 0) throw new Exception("Wrapable height not set");
        }
        public void WrapPosition()
        {
            Vector2 pos = base.position;
            while (pos.X < 0)
            {
                pos.X += width;
            }
            while (pos.X > width)
            {
                pos.X -= width;
            }

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
