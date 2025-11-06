using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal class ParticleLine : Particle
    {
        private readonly float length;
        private readonly int thickness;
        public ParticleLine(object[] args, Vector2 position, Vector2 velocity, float angularVelocity, float lifetime, float rotation, (Color color, float t)[] gradient = null!) : base(position, velocity, angularVelocity, lifetime, rotation, gradient)
        {
            this.length = (float)args[0];
            this.thickness = (int)args[1];
        }

        public ParticleLine(object[] args, Vector2 position, Vector2 velocity, (float Min, float Max) angularVelocity, float lifetime, float rotation, (Color color, float t)[] gradient = null!) : base(position, velocity, angularVelocity, lifetime, rotation, gradient)
        {
            this.length = (float)args[0];
            this.thickness = (int)args[1];
        }

        /// <summary>
        /// The draw procedure for the <see cref="ParticleDot"/> class
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw to</param>
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
