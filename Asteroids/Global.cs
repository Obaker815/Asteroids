using Asteroids.Entities;
using Asteroids.JSONs;
using Asteroids.Menus;
using System.Numerics;
using System.Web;

namespace Asteroids
{
    internal static class Global
    {
        public static bool DEMO_ENABLED = true;
        public static bool DEBUG = false;
        public static bool FPSDISPLAY = false;
        public static bool DEBUG_PARTICLE_DRAW = false;
        public const float DEBUG_DIRECTION_LINE_LENGTH = 3f;
        public const bool PLAYER_COLLISION = true;

        public const string DATA_PATH = "./data";
        public const string CONFIG_PATH = "/config.json";
        public const string SCOREBOARD_PATH = "/scoreboard.json";
        public const string KEYBIND_PATH_BASE = "/Keybinds/";
        public const string FONT_PATH_BASE = "/Fonts/";

        public const string DEFAULT_KEYBIND_FILE = "default_keybinds.json";

        public static bool SUPPRESS_OPTION_CHANGED_EVENT = false;
        public static Configs CONFIGS = null!;
        public static GameState PREVIOUS_STATE = GameState.None;
        public static GameState CURRENT_STATE  = GameState.MainMenu;
        public static Dictionary<GameState, IMenu> STATE_MENU = new()
        {
            {GameState.MainMenu,        null!},
            {GameState.SettingsMenu,    null!},
            {GameState.KeybindsMenu,    null!},
            {GameState.Playing,         null!},
            {GameState.None,            null!},
            {GameState.NameEntryMenu,   null!},
        };
        public static Dictionary<GameState, GameState> STATE_RETURN = new()
        {
            {GameState.MainMenu,        GameState.None},
            {GameState.SettingsMenu,    GameState.MainMenu},
            {GameState.KeybindsMenu,    GameState.SettingsMenu},
            {GameState.Playing,         GameState.MainMenu},
            {GameState.NameEntryMenu,   GameState.NameEntryMenu },
            {GameState.None,            GameState.None},
        };

        public static void GameStart()
        {
            Entity.Entities.Clear();
            Wrapable.Wrapables.Clear();
            Asteroid.AsteroidEntities.Clear();
            Saucer.Saucers.Clear();
            Bullet.Bullets.Clear();
            Ship.Ships.Clear();

            if (DEMO_ENABLED)
                _ = new DemoShip(new Vector2(
                    GameForm.preferredRect.Width / 2, 
                    GameForm.preferredRect.Height / 2));
            else
                _ = new Ship(new Vector2(
                    GameForm.preferredRect.Width / 2, 
                    GameForm.preferredRect.Height / 2));

            LevelManager.Instance = new();
        }

        /// <summary>
        /// A safe normalization function for <see cref="Vector2"/> values
        /// </summary>
        /// <param name="v">The input vector</param>
        /// <returns>
        /// A normalized <see cref="Vector2"/> pointing in the same direction as <paramref name="v"/>,  
        /// or <see cref="Vector2.Zero"/> if the input is zero or contains <see cref="float.NaN"/> components.
        /// </returns>
        public static Vector2 Normalize(Vector2 v)
        {
            if (v == Vector2.Zero) return Vector2.Zero;
            if (float.IsNaN(v.X) || float.IsNaN(v.Y)) return Vector2.Zero;
            return Vector2.Normalize(v);
        }

        /// <summary>
        /// A two dimensional linear interpolation function <see cref="Vector2"/> values
        /// </summary>
        /// <param name="a">The start vector</param>
        /// <param name="b">The end vector</param>
        /// <param name="t">The interpolation factor</param>
        /// <returns>The interpolated <see cref="Vector2"/> value between <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        }

        /// <summary>
        /// A linear interpolation function of <see cref="float"/> values
        /// </summary>
        /// <param name="a">The start value</param>
        /// <param name="b">The end value</param>
        /// <param name="t">The interpolation factor</param>
        /// <returns>The interpolated <see cref="float"/> value between <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static string GetFileName(string path)
        {
            int endIndex = path.LastIndexOf('.');
            int startIndex = path.LastIndexOf('/') + 1;
            return path[startIndex..endIndex];
        }
    }
}
