using System.Numerics;

namespace Asteroids.Particles.Particles
{
    internal class Dot : Particle
    {
        public Dot(object[] args,
                           Vector2 position,
                           Vector2 velocity,
                           float angularVelocity,
                           float lifetime,
                           float rotation,
                           (Color color, float t)[] gradient) : base(position, 
                               velocity, 
                               angularVelocity, 
                               lifetime, 
                               rotation, 
                               gradient)
        {
            Particles.Add(this);
            _ = args;
        }

        /// <summary>
        /// The draw procedure for the <see cref="Dot"/> class
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw to</param>
        public override void Draw(Graphics g)
        {
            float progress = age / lifetime;
            Color c = GetColor(progress);
            g.FillRectangle(new SolidBrush(c), position.X, position.Y, 2, 2);
        }
    }
}
