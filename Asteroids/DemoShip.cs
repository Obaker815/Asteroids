using SharpDX.XInput;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace Asteroids
{
    internal class DemoShip : Ship
    {
        public DemoShip(Vector2 startPosition) : base(startPosition)
        {
        }

        internal override void Respawn()
        {
            lives++;
            base.Respawn();
        }

        public override void Update(Dictionary<string, Keybind> Keys, Controller controller, float dt)
        {
            if (respawning)
            {
                GameForm.AddFreezeTime(time: 0.5f, modifier: 0.5f);
                position = respawnLocation;
                accelerating = false;

                respawnTime -= dt;
                if (respawnTime <= 0f)
                {
                    respawning = false;
                    respawnTime = 0f;
                    iFrames = 1f;
                }

                return;
            }
            lookDir = Vector2.Transform(lookDir, Matrix3x2.CreateRotation(ANGULAR_ACCELERATION * dt));

            velocity += lookDir * ACCELERATION * dt;

            if (base.velocity.LengthSquared() > MAX_VELOCITY * MAX_VELOCITY)
                base.velocity = Global.Normalize(base.velocity) * MAX_VELOCITY;

            void collisionHandle(Entity collided)
            {
                if (collided is not Ship)
                {
                    toRemove.Add(collided);
                    Respawn();
                }
            }

            if (iFrames > 0 || !Global.PLAYER_COLLISION)
            {
                iFrames -= dt;
                iFrames = float.Max(0, iFrames);
            } else
            {
                Entity? collidedBullet = CollisionCheck(this, typeof(Bullet));
                if (collidedBullet is not null)
                {
                    Bullet? b = collidedBullet as Bullet;
                    if (b?.Parent != this)
                    {
                        collisionHandle(collidedBullet);
                    }
                }

                Entity? collidedAsteroid = CollisionCheck(this, typeof(Asteroid));
                if (collidedAsteroid is not null)
                {
                    collisionHandle(collidedAsteroid);
                }

                Entity? collidedSaucer = CollisionCheck(this, typeof(Saucer));
                if (collidedSaucer is not null)
                {
                    collisionHandle(collidedSaucer);
                }
            }
        }
    }
}
