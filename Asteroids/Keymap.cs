using System.Text.Json.Serialization;

namespace Asteroids
{
    internal class Keymap
    {
        [JsonPropertyName("accelerate")]
        public string Accelerate { get; set; } = "W";

        [JsonPropertyName("right")]
        public string Right { get; set; } = "D";

        [JsonPropertyName("left")]
        public string Left { get; set; } = "A";

        [JsonPropertyName("upDirection")]
        public string UpDirection { get; set; } = "I";

        [JsonPropertyName("downDirection")]
        public string DownDirection { get; set; } = "K";

        [JsonPropertyName("rightDirection")]
        public string RightDirection { get; set; } = "L";

        [JsonPropertyName("leftDirection")]
        public string LeftDirection { get; set; } = "J";

        [JsonPropertyName("shoot")]
        public string Shoot { get; set; } = "Space";

        public Dictionary<string, Keybind> keybinds;

        public Keymap()
        {
            keybinds = ToDictionary();
        }

        public Dictionary<string, Keybind> ToDictionary()
        {
            return new Dictionary<string, Keybind>
            {
                { "Up",         new Keybind((Keys)Enum.Parse(typeof(Keys), Accelerate)) },
                { "Left",       new Keybind((Keys)Enum.Parse(typeof(Keys), Left)) },
                { "Right",      new Keybind((Keys)Enum.Parse(typeof(Keys), Right)) },
                { "UpAlt",      new Keybind((Keys)Enum.Parse(typeof(Keys), UpDirection)) },
                { "DownAlt",    new Keybind((Keys)Enum.Parse(typeof(Keys), DownDirection)) },
                { "LeftAlt",    new Keybind((Keys)Enum.Parse(typeof(Keys), LeftDirection)) },
                { "RightAlt",   new Keybind((Keys)Enum.Parse(typeof(Keys), RightDirection)) },
                { "Shoot",      new Keybind((Keys)Enum.Parse(typeof(Keys), Shoot)) },
            };
        }

        public void FromDictionary(Dictionary<string, Keybind> dict)
        {
            if (dict.TryGetValue("Up", out var upKeybind))          Accelerate = upKeybind.Key.ToString();
            if (dict.TryGetValue("Left", out var leftKeybind))      Left = leftKeybind.Key.ToString();
            if (dict.TryGetValue("Right", out var rightKeybind))    Right = rightKeybind.Key.ToString();

            if (dict.TryGetValue("UpAlt", out var upAltKeybind))        UpDirection = upAltKeybind.Key.ToString();
            if (dict.TryGetValue("DownAlt", out var downAltKeybind))    DownDirection = downAltKeybind.Key.ToString();
            if (dict.TryGetValue("LeftAlt", out var leftAltKeybind))    LeftDirection = leftAltKeybind.Key.ToString();
            if (dict.TryGetValue("RightAlt", out var rightAltKeybind))  RightDirection = rightAltKeybind.Key.ToString();

            if (dict.TryGetValue("Shoot", out var shootKeybind)) Shoot = shootKeybind.Key.ToString();
        }
    }
}
