using System.Text.Json.Serialization;

namespace Asteroids
{
    internal class ConfigsJSON
    {
        [JsonPropertyName("Fullscreen")]
        public bool Fullscreen { get; set; }

        [JsonPropertyName("ControlStyle")]
        public int ControlStyle { get; set; }

        [JsonPropertyName("FPS")]
        public int FPS { get; set; }

        [JsonPropertyName("DebugAvailable")]
        public bool DebugAvailable { get; set; }
    }
}
