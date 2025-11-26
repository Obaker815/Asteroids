namespace Asteroids
{
    internal static class JSONReader
    {
        private static readonly System.Text.Json.JsonSerializerOptions options = new() { WriteIndented = true };
        public static void WriteJson(string filePath, object obj)
        {
            string jsonString = System.Text.Json.JsonSerializer.Serialize(obj, options);
            File.WriteAllText(filePath, jsonString);
        }
        public static T ReadJson<T>(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return System.Text.Json.JsonSerializer.Deserialize<T>(jsonString)!;
        }
    }
}
