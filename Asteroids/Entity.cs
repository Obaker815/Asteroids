using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Entity
    {
        public static List<Entity> toRemove = new List<Entity>();
        public static List<Entity> Entities = [];
        internal Vector2 position;
        internal Vector2 velocity;
        internal float radius = 0;

        /// <summary>
        /// Constructor for the Entity class
        /// </summary>
        /// <param name="startPosition">The starting position of the Entity</param>
        public Entity(Vector2 startPosition)
        {
            position = startPosition;
            velocity = Vector2.Zero;
            Entities.Add(this);
        }

        public void Update(float dt)
        {
            if (velocity.LengthSquared() < 0.01f)
                velocity = Vector2.Zero;
            position += velocity * dt;
        }

        public virtual void Remove()
        {
            Entities.Remove(this);
        }

        public static Entity? CollisionCheck(Entity entity)
        {
            Entity[] entities = Entities.ToArray();
            foreach (Entity e in entities)
            {
                if (e == entity) continue;

                float distSq = Vector2.DistanceSquared(entity.position, e.position);
                float radiusSum = entity.radius + e.radius;

                if (distSq <= radiusSum * radiusSum)
                {
                    return e;
                }
            }

            return null;
        }
    }
}
