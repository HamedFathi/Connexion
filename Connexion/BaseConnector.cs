using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Connexion
{
    public abstract class BaseConnector
    {
        public abstract string Id { get; }
        public string Location { get; } = Assembly.GetCallingAssembly().Location;
        public abstract JObject Execute(JObject metadata, ConnectorInfo basicInfo);
        public void SaveSettings(JObject metadata, ConnectorInfo basicConnectorInfo)
        {
            File.WriteAllTextAsync(Path.Combine(basicConnectorInfo.Destination, "metadata.json"), metadata.ToString());
        }

        protected JToken ReadMetadata(string id, JObject metadata)
        {
            return metadata.ContainsKey(id) ? metadata[id] : null;
        }

        protected JObject SetMetadata(object obj, JObject metadata)
        {
            metadata[Id] = JObject.FromObject(obj);
            return metadata;
        }
    }
}
