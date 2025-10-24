﻿using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Bullet : Wrapable, IDisposable
    {
        public static List<Bullet> Bullets = [];

        private readonly int removeTime = 1000;
        private readonly Stopwatch lifeTimer;
        private bool disposing = false;

        public readonly object parent;

        /// <summary>
        /// The constructor for the <see cref="Bullet"> class
        /// </summary>
        /// <param name="Position">The <see cref="Vector2"/> position for the <see cref="Bullet"/> to start</param>
        /// <param name="InitialVelocity">The <see cref="Vector2"/> velocity for the <see cref="Bullet"/> to start with</param>
        /// <param name="Direction">The <see cref="Vector2"/> direction of travel for the <see cref="Bullet"/></param>
        /// <param name="speed">The <see cref="float"/> speed of the <see cref="Bullet"/></param>
        /// <param name="RemoveTime">the <see cref="int"> lifetime of the <see cref="Bullet"/> in milliseconds</param>
        /// <param name="Parent">the <see cref="object"/> parent of the <see cref="Bullet"/></param>
        public Bullet(Vector2 Position, Vector2 InitialVelocity, Vector2 Direction, float speed, int RemoveTime, object Parent) : base(Position)
        {
            velocity = Direction * speed + InitialVelocity;
            removeTime = RemoveTime;
            parent = Parent;
            radius = 2;

            Bullets.Add(this);

            lifeTimer = Stopwatch.StartNew();
        }

        /// <summary>
        /// The update method of the <see cref="Bullet"/> class
        /// </summary>
        public void Update()
        {
            if (disposing) return;
            if (lifeTimer.Elapsed.TotalMilliseconds >= removeTime)
            {
                Dispose();
            }
        }

        /// <summary>
        /// The override draw method for the <see cref="Bullet"/> class
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> to be drawn to</param>
        /// <param name="Position">The <see cref="Vector2"/> position to be drawn</param>
        public override void Draw(Graphics g, Vector2 Position)
        {
            g.DrawEllipse(Pens.White, Position.X - radius, Position.Y - radius, 2 * radius, 2 * radius);
        }

        /// <summary>
        /// Dispose method for the <see cref="Bullet"/> class
        /// </summary>
        public void Dispose()
        {
            if (disposing) return;
            disposing = true;

            toRemove.Add(this);
            lifeTimer.Stop();
        }

        public override void Remove()
        {
            if (parent is Ship p)
            {
                p.numBullets--;
            }

            Bullets.Remove(this);
            base.Remove();
        }
    }
}
