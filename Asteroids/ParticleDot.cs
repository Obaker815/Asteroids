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
        
        public override void Draw(Graphics g)
        {
            float progress = age / lifetime;
            Color c = GetColor(progress);
            g.FillRectangle(new SolidBrush(c), position.X, position.Y, 2, 2);

        }
    }
}
