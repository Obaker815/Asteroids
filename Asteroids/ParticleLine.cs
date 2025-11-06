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
        public ParticleLine(Vector2 position, Vector2 velocity, (float Min, float Max) angularVelocity, float lifetime, float length, int thickness, (Color color, float t)[] gradient = null!) : base(position, velocity, angularVelocity, lifetime, gradient)
        {
            this.length = length;
            this.thickness = thickness;
        }

        public ParticleLine(Vector2 position, Vector2 velocity, (float Min, float Max) angularVelocity, float lifetime, (Color color, float t)[] gradient = null!) : base(position, velocity, angularVelocity, lifetime, gradient)
        {
            this.length = 10;
            this.thickness = 2;
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
