using Newtonsoft.Json.Linq;

namespace Connexion
{
    public static class Extensions
    {
        public static bool ContainsKey(this JToken jToken, string key)
        {
            if (jToken == null) return false;
            return jToken[key] != null;
        }
        public static string GetAsString(this JToken jToken, string key)
        {
            if (jToken == null) return null;
            if (string.IsNullOrEmpty(key)) return null;
            return jToken.ContainsKey(key) ? jToken[key].ToString() : null;
        }
    }
}
