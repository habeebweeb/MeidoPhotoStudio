using MeidoPhotoStudio.Plugin.Core.Extension;
using MeidoPhotoStudio.Plugin.Core.Schema.Extension;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class ExtensionSchemaBuilder : ISceneSchemaAspectBuilder<ExtensionSchema>
{
    private readonly Dictionary<string, IExtensionDataBuilder> schemaBuilders = [];

    public bool RegisterSchemaBuilder(string extensionID, IExtensionDataBuilder schemaBuilder)
    {
        if (string.IsNullOrEmpty(extensionID))
            throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID));

        _ = schemaBuilder ?? throw new ArgumentNullException(nameof(schemaBuilder));

        if (schemaBuilders.ContainsKey(extensionID))
        {
            Plugin.Logger.LogWarning($"Extension schema builder with ID '{extensionID}' is already registered.");

            return false;
        }

        schemaBuilders[extensionID] = schemaBuilder;

        return true;
    }

    public bool DeregisterSchemaBuilder(string extensionID) =>
        string.IsNullOrEmpty(extensionID)
            ? throw new ArgumentException($"'{nameof(extensionID)}' cannot be null or empty.", nameof(extensionID))
            : schemaBuilders.Remove(extensionID);

    public ExtensionSchema Build()
    {
        return new()
        {
            ExtensionData = [.. schemaBuilders.Select(SafeBuildData).Where(static data => data is not null)],
        };

        static IExtensionData SafeBuildData(KeyValuePair<string, IExtensionDataBuilder> kvp)
        {
            var (id, builder) = kvp;

            IExtensionData data = null;

            try
            {
                data = builder.Build();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Extension schema builder with ID '{id}' threw unhandled exception.\n{e}");
            }

            return data;
        }
    }
}
