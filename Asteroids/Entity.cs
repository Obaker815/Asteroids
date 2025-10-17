using System.Numerics;

namespace Asteroids
{
    internal class Entity
    {
        public static List<Entity> Entities = new List<Entity>();
        internal Vector2 position;
        internal Vector2 velocity;

        /// <summary>
        /// Constructor for the Entity class
        /// </summary>
        /// <param name="startPosition">The starting position of the Entity</param>
        public Entity(Vector2 startPosition)
        {
            position = startPosition;
            velocity = Vector2.Zero;
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
