using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Entity
    {
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
            if (velocity.LengthSquared() < 0.001f)
                velocity = Vector2.Zero;
            position += velocity * dt;
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set
            {
                // implement value validation later
                velocity = value;
            }
        }
    }
}
