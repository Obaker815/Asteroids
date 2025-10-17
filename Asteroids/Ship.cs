using System.Numerics;

namespace Asteroids
{
    internal class Ship : Wrapable
    {
        public static List<Ship> ships = [];
        public Ship(Vector2 startPosition) : base(startPosition)
        {
            ships.Add(this);
        }
        
        /// <summary>
        /// Override Draw method from Wrapable
        /// </summary>
        /// <param name="g">Graphics object to be drawn to</param>
        /// <param name="position">Position to be drawn</param>
        public override void Draw(Graphics g, Vector2 position)
        {
            g.FillEllipse(Brushes.White, position.X - 10, position.Y - 10, 20, 20);
        }
    }
}
