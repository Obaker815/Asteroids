using System.Numerics;

namespace Asteroids
{
    internal class ParticleImage : Particle
    {
        private readonly Image image;
        private readonly float? scale;
        public ParticleImage(object[] args,
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
            if (args.Length != 2) throw new Exception("Incorrect arguments for ParticleImage"); 

            Particles.Add(this);
            image = (args[0] as Image)!;
            scale = (args[1] as float?)!;
        }

        /// <summary>
        /// The draw procedure for the <see cref="ParticleImage"/> class
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw to</param>
        public override void Draw(Graphics g)
        {
            if (image is null) return;
            g.DrawImage(image, position.X, position.Y, (float)(image.Width * scale)!, (float)(image.Height * scale)!);
        }
    }
}
