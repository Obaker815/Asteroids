using System.Numerics;

namespace Asteroids
{
    internal class Entity
    {
        public static List<Entity> toRemove = [];
        public static List<Entity> Entities = [];
        internal Vector2 position;
        internal Vector2 velocity;
        internal float radius = 0;

        /// <summary>
        /// Constructor for the <see cref="Entity"> class
        /// </summary>
        /// <param name="startPosition">The starting position of the Entity</param>
        public Entity(Vector2 startPosition)
        {
            position = startPosition;
            velocity = Vector2.Zero;
            Entities.Add(this);
        }

        /// <summary>
        /// The update method for the <see cref="Entity"> class
        /// </summary>
        /// <param name="dt">Deltatime in seconds</param>
        public void Update(float dt)
        {
            if (velocity.LengthSquared() < 1f * dt)
                velocity = Vector2.Zero;
            position += velocity * dt;
        }

        /// <summary>
        /// Checks for collision between the given <see cref="Entity"> and all other <see cref="Entity"> in <see cref="Entities">
        /// </summary>
        /// <param name="entity">The <see cref="Entity"> to be compared</param>
        /// <returns>The first <see cref="Entity"> in <see cref="Entities"> to overlap <paramref name="entity"/></returns>
        public static Entity? CollisionCheck(Entity entity, Type targetType)
        {
            Entity[] entities = [.. Entities];
            foreach (Entity e in entities)
            {
                if (e == entity || e is null || e.GetType() != targetType) continue;

                float distSq = Vector2.DistanceSquared(entity.position, e.position);
                float radiusSum = entity.radius + e.radius;

                if (distSq <= radiusSum * radiusSum)
                {
                    return e;
                }
            }

            return null;
        }

        public virtual void Remove()
        {
            Entities.Remove(this);
        }
    }
}
