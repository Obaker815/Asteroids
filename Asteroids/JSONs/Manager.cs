using System.Text.Json;

namespace Asteroids.JSONs
{
    internal static class Manager
    {
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true, AllowTrailingCommas = true };

        /// <summary>
        /// Writes a given JSON (<paramref name="obj"/>) to the <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">The path to Write the <paramref name="obj"/> to</param>
        /// <param name="obj">The JSON to be written</param>
        public static void WriteJson(string filePath, object obj)
        {
            string jsonString = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Reads the JSON file from <paramref name="filePath"/>, with type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to read from the <paramref name="filePath"/></typeparam>
        /// <param name="filePath">The path to the <typeparamref name="T"/> JSON</param>
        /// <returns></returns>
        public static T ReadJson<T>(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(jsonString)!;
        }
    }
}
