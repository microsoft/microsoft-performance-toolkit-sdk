using Microsoft.Performance.SDK.Processing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class PluginManifestJsonValidator
        : IPluginManifestFileValidator
    {
        private readonly JSchema schema;
        private readonly ILogger logger;

        public PluginManifestJsonValidator(string schemaString, Func<Type, ILogger> loggerFactory)
        {
            this.schema = JSchema.Parse(schemaString);
            this.logger = loggerFactory(typeof(PluginManifestJsonValidator));
        }

        public bool Validate(string pluginManifestPath)
        {
            var jsonData = JToken.Parse(File.ReadAllText(pluginManifestPath));
            bool isValid = jsonData.IsValid(this.schema, out IList<string> errors);

            if (!isValid)
            {
                foreach (string error in errors)
                {
                    this.logger.Error($"Plugin manifest validation error: {error}");
                }
            }

            return isValid;
        }
    }
}
