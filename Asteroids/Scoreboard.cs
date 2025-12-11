using System.Text.Json.Serialization;

namespace Asteroids
{
    internal class Scoreboard
    {
        [JsonPropertyName("ScoreboardEntries")]
        public ScoreboardEntry[]? Entries { get; set; }
    }
    internal class ScoreboardEntry
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Score")]
        public int Score { get; set; }
    }
}
