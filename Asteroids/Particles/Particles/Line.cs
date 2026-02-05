using System.Numerics;

namespace Asteroids.Particles.Particles
{
    internal class Line : Particle
    {
        private readonly float length;
        private readonly int thickness;
        public Line(object[] args, 
                Vector2 position, 
                Vector2 velocity, 
                float angularVelocity, 
                float lifetime, 
                float rotation, 
                (Color color, float t)[] gradient = null!) : base(position, 
                    velocity, 
                    angularVelocity, 
                    lifetime, 
                    rotation, 
                    gradient)
        {
            if (args.Length != 2)       throw new Exception("Incorrect arguments for particleline");
            if (args[0] is not float)   throw new Exception("args[0] should be float for particleline");
            if (args[1] is not int)     throw new Exception("args[1] should be int for particleline");

            this.length = (float)args[0];
            this.thickness = (int)args[1];
        }

        /// <summary>
        /// the draw procedure for the <see cref="particledot"/> class
        /// </summary>
        /// <param name="g">the <see cref="graphics"/> object to draw to</param>
        public override void Draw(Graphics g)
        {
            float progress = age / lifetime;
            Color c = GetColor(progress);

            Vector2 start = position + Vector2.Transform(new(+length / 2, 0), Matrix3x2.CreateRotation(rotation));
            Vector2 end   = position + Vector2.Transform(new(-length / 2, 0), Matrix3x2.CreateRotation(rotation));

            g.DrawLine(new Pen(c, thickness), new PointF(start), new PointF(end));
        }
    }
}
