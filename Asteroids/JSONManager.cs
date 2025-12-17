using System.Diagnostics;
using System.Text.Json;

namespace Asteroids
{
    internal static class JSONManager
    {
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true, AllowTrailingCommas = true };
        public static void WriteJson(string filePath, object obj)
        {
            string jsonString = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(filePath, jsonString);
        }
        public static T ReadJson<T>(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(jsonString)!;
        }
    }
}
