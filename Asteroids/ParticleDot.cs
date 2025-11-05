using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal class ParticleDot : Particle
    {
        public ParticleDot(Vector2 Position,
                           Vector2 Velocity,
                           float AngularVelocity,
                           float Lifetime)
             : base(Position,
                    Velocity,
                    AngularVelocity,
                    Lifetime)
        {
            Particles.Add(this);
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.White, position.X, position.Y, 2, 2);
        }
    }
}
