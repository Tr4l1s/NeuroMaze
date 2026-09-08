// Test-only adapter: never copied into Assets. Exercises the real record/export code.
using System;
using System.IO;
using System.Text.Json;
namespace UnityEngine
{
    public static class Application
    {
        public static string persistentDataPath = Path.Combine(Directory.GetCurrentDirectory(), "work", "pulse", "Tests", "test-data", Guid.NewGuid().ToString("N"));
    }
    public static class JsonUtility
    {
        public static string ToJson(object value, bool pretty) { return JsonSerializer.Serialize(value, new JsonSerializerOptions { IncludeFields = true, WriteIndented = pretty }); }
        public static T FromJson<T>(string value) { return JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions { IncludeFields = true }); }
    }
}
