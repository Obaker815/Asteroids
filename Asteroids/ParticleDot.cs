using System.Numerics;

namespace Asteroids
{
    internal class ParticleDot : Particle
    {
        public ParticleDot(object[] args,
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
            if (args.Length != 0) throw new Exception("Incorrect arguments for ParticleDot"); 

            Particles.Add(this);
            _ = args;
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
