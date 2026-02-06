using SharpDX.XInput;
using System.Numerics;

namespace Asteroids.Entities
{
    internal class DemoShip(Vector2 startPosition) : Ship(startPosition)
    {
        private const float SCARE_DISTANCE = 135f;
        private const float SHOOT_COOLDOWN = 0.5f;
        private float _shootTime = SHOOT_COOLDOWN;

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

            if (_shootTime > 0)
                _shootTime -= dt;

            Vector2 closestWrapable = new(0, -100000000);
            Wrapable? closestWrapableObject = null;

            Wrapable[] wrappers = [.. Wrapable.Wrapables];
            foreach(var wrapper in wrappers)
            {
                if (wrapper as DemoShip == this) continue;
                if ((wrapper as Bullet)?.Parent == this) continue;
                if (wrapper is Bullet && Vector2.Dot(wrapper.velocity, velocity) > 0.2) continue;

                Vector2 closest = wrapper.GetClosest(position);
                float distOld = Vector2.DistanceSquared(position, closestWrapable);
                float distNew = Vector2.DistanceSquared(position, closest);

                if (distNew < distOld || closestWrapableObject == null)
                {
                    closestWrapableObject = wrapper;
                    closestWrapable = closest;
                }
            }

            Vector2 offset = closestWrapable - position;
            bool scared = offset.LengthSquared() < SCARE_DISTANCE * SCARE_DISTANCE;

            float extraSpeed = 0;

            if (closestWrapableObject is Bullet)
            {
                scared = true;
                offset = Global.Normalize(closestWrapableObject.velocity);
                offset.X *= -1;
                (offset.X, offset.Y) = (offset.Y, offset.X);
            }
            if (closestWrapableObject != null)
            {
                extraSpeed = closestWrapableObject.velocity.Length();
                if (!scared)
                {
                    Vector2 velocity = closestWrapableObject.velocity;
                    offset += velocity;
                }
            }

            Vector2 targetDir;

            // scared
            if (scared) targetDir = -Global.Normalize(offset);
            else targetDir = Global.Normalize(offset);

            lookDir += (targetDir - lookDir) * dt * ANGULAR_ACCELERATION;
            lookDir = Global.Normalize(lookDir);

            if (scared && Vector2.Dot(lookDir, targetDir) > 0.2)
                velocity += lookDir * ACCELERATION * dt;
            else if (closestWrapableObject != null && Vector2.Dot(targetDir, lookDir) > 0 && _shootTime <= 0)
            {
                _shootTime = (_shootTime + SHOOT_COOLDOWN) % SHOOT_COOLDOWN;
                Shoot();
            }

            float maxVel = (MAX_VELOCITY + extraSpeed) / 4;

            if (base.velocity.LengthSquared() > maxVel * maxVel)
                base.velocity = Global.Normalize(base.velocity) * maxVel;


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
