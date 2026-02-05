using System.Text.Json.Serialization;

namespace Asteroids.JSONs
{
    internal class Scoreboard
    {
        [JsonPropertyName("ScoreboardEntries")]
        public ScoreboardEntry[]? Entries { get; set; }

        public void SortEntries()
        {
            if (Entries == null) return;
            Entries = Sorting.Bubble(
                ToSort: Entries,
                GetValue: (a) => { return a.Score; },
                inverse: true);

            Entries = Entries[.. Math.Min(Entries.Length, 10)];
        }
    }
    internal class ScoreboardEntry
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Score")]
        public int Score { get; set; }
    }
}
