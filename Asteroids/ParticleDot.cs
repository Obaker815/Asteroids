using System.Numerics;

namespace Asteroids
{
    internal class ParticleDot : Particle
    {
        public ParticleDot(Vector2 Position,
                           Vector2 Velocity,
                           float AngularVelocity,
                           float Lifetime,
                           (Color color, float t)[] gradient)
            : base(Position, Velocity, AngularVelocity, Lifetime, gradient)
        {
            Particles.Add(this);
        }

        /// <summary>
        /// The draw procedure for the <see cref="ParticleDot"/> class
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
