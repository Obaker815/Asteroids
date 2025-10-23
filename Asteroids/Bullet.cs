using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Bullet : Wrapable, IDisposable
    {
        public static List<Bullet> Bullets = new List<Bullet>();

        public object parent;
        Stopwatch lifeTimer;
        private int removeTime = 1000;
        public Bullet(Vector2 Position, Vector2 InitialVelocity, Vector2 Direction, float speed, int RemoveTime, object Parent) : base(Position)
        {
            velocity = Direction * speed + InitialVelocity;
            removeTime = RemoveTime;
            parent = Parent;
            radius = 2;

            Bullets.Add(this);

            lifeTimer = Stopwatch.StartNew();
        }
        public void Update()
        {
            if (disposing) return;
            if (lifeTimer.Elapsed.TotalMilliseconds >= removeTime)
            {
                Dispose();
            }
        }

        public override void Draw(Graphics g, Vector2 Position)
        {
            g.DrawEllipse(Pens.White, Position.X - radius, Position.Y - radius, 2 * radius, 2 * radius);
        }

        private bool disposing = false;
        public void Dispose()
        {
            if (disposing) return;
            disposing = true;

            if (parent is Ship p)
            {
                p.numBullets--;
            }

            toRemove.Add(this);
            lifeTimer.Stop();
        }
    }
}
