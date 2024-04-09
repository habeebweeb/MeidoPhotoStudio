using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class FaceShapeKeyConfiguration
{
    private static readonly string[] DefaultBlockList =
    [
        "earelf", "earnone", "eyebig", "eyeclose", "eyeclose1_normal", "eyeclose1_tare", "eyeclose1_tsuri", "eyeclose2",
        "eyeclose2_normal", "eyeclose2_tare", "eyeclose2_tsuri", "eyeclose3", "eyeclose5", "eyeclose5_normal",
        "eyeclose5_tare", "eyeclose5_tsuri", "eyeclose6", "eyeclose6_normal", "eyeclose6_tare", "eyeclose6_tsuri",
        "eyeclose7", "eyeclose7_normal", "eyeclose7_tare", "eyeclose7_tsuri", "eyeclose8", "eyeclose8_normal",
        "eyeclose8_tare", "eyeclose8_tsuri", "eyeeditl1_dw", "eyeeditl1_up", "eyeeditl2_dw", "eyeeditl2_up",
        "eyeeditl3_dw", "eyeeditl3_up", "eyeeditl4_dw", "eyeeditl4_up", "eyeeditl5_dw", "eyeeditl5_up", "eyeeditl6_dw",
        "eyeeditl6_up", "eyeeditl7_dw", "eyeeditl7_up", "eyeeditl8_dw", "eyeeditl8_up", "eyeeditr1_dw", "eyeeditr1_up",
        "eyeeditr2_dw", "eyeeditr2_up", "eyeeditr3_dw", "eyeeditr3_up", "eyeeditr4_dw", "eyeeditr4_up", "eyeeditr5_dw",
        "eyeeditr5_up", "eyeeditr6_dw", "eyeeditr6_up", "eyeeditr7_dw", "eyeeditr7_up", "eyeeditr8_dw", "eyeeditr8_up",
        "hitomih", "hitomis", "hoho", "hoho2", "hohol", "hohos", "mayueditin", "mayueditout", "mayuha", "mayuup",
        "mayuv", "mayuvhalf", "mayuw", "moutha", "mouthc", "mouthdw", "mouthfera", "mouthferar", "mouthhe", "mouthi",
        "mouths", "mouthup", "mouthuphalf", "namida", "nosefook", "shape", "shapehoho", "shapehohopushr", "shapeslim",
        "shock", "tangopen", "tangout", "tangup", "tear1", "tear2", "tear3", "toothoff", "yodare",
    ];

    private readonly ConfigFile configFile;
    private readonly ConfigEntry<ShapeKeyCollection> customShapeKeysConfigEntry;
    private readonly ConfigEntry<ShapeKeyCollection> blockedShapeKeysConfigEntry;

    public FaceShapeKeyConfiguration(ConfigFile configFile)
    {
        this.configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));

        customShapeKeysConfigEntry = this.configFile.Bind("Character", "Custom Face Shape Keys", new ShapeKeyCollection());
        blockedShapeKeysConfigEntry = this.configFile.Bind("Character", "Shape Key Block List", new ShapeKeyCollection(DefaultBlockList));
    }

    public event EventHandler<FaceShapeKeyConfigurationEventArgs> AddedCustomShapeKey;

    public event EventHandler<FaceShapeKeyConfigurationEventArgs> RemovedCustomShapeKey;

    public event EventHandler<FaceShapeKeyConfigurationEventArgs> BlockedShapeKey;

    public event EventHandler<FaceShapeKeyConfigurationEventArgs> UnblockedShapeKey;

    public IEnumerable<string> CustomShapeKeys =>
        customShapeKeysConfigEntry.Value;

    public IEnumerable<string> BlockedShapeKeys =>
        blockedShapeKeysConfigEntry.Value;

    public bool AddCustomShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!customShapeKeysConfigEntry.Value.Add(shapeKey))
            return false;

        AddedCustomShapeKey?.Invoke(this, new(shapeKey));

        return true;
    }

    public void RemoveCustomShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!customShapeKeysConfigEntry.Value.Remove(shapeKey))
            return;

        RemovedCustomShapeKey?.Invoke(this, new(shapeKey));
    }

    public bool BlockShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentNullException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!blockedShapeKeysConfigEntry.Value.Add(shapeKey))
            return false;

        BlockedShapeKey?.Invoke(this, new(shapeKey));

        return true;
    }

    public void UnblockShapeKey(string shapeKey)
    {
        if (string.IsNullOrEmpty(shapeKey))
            throw new ArgumentNullException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

        if (!blockedShapeKeysConfigEntry.Value.Remove(shapeKey))
            return;

        UnblockedShapeKey?.Invoke(this, new(shapeKey));
    }

    private class ShapeKeyCollection : IEnumerable<string>
    {
        private readonly List<string> shapeKeys;

        static ShapeKeyCollection() =>
            TomlTypeConverter.AddConverter(
                typeof(ShapeKeyCollection),
                new()
                {
                    ConvertToString = (shapeKeyCollection, _) => ((ShapeKeyCollection)shapeKeyCollection).Serialize(),
                    ConvertToObject = (data, _) => Deserialize(data),
                });

        public ShapeKeyCollection() =>
            shapeKeys = [];

        public ShapeKeyCollection(IEnumerable<string> values)
        {
            shapeKeys = [.. values ?? throw new ArgumentNullException(nameof(values))];

            shapeKeys.Sort(StringComparer.Ordinal);
        }

        public bool Add(string shapeKey)
        {
            if (string.IsNullOrEmpty(shapeKey))
                throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey));

            if (shapeKeys.Count is 0)
            {
                shapeKeys.Add(shapeKey);

                return true;
            }
            else if (string.CompareOrdinal(shapeKeys[shapeKeys.Count - 1], shapeKey) < 0)
            {
                shapeKeys.Add(shapeKey);

                return true;
            }
            else if (string.CompareOrdinal(shapeKeys[0], shapeKey) > 0)
            {
                shapeKeys.Insert(0, shapeKey);

                return true;
            }
            else
            {
                var index = shapeKeys.BinarySearch(shapeKey);

                if (index >= 0)
                    return false;

                shapeKeys.Insert(~index, shapeKey);

                return true;
            }
        }

        public bool Remove(string shapeKey) =>
            string.IsNullOrEmpty(shapeKey)
                ? throw new ArgumentException($"'{nameof(shapeKey)}' cannot be null or empty.", nameof(shapeKey))
                : shapeKeys.Remove(shapeKey);

        public IEnumerator<string> GetEnumerator() =>
            ((IEnumerable<string>)shapeKeys).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private static ShapeKeyCollection Deserialize(string data) =>
            new(data.Split([','], StringSplitOptions.RemoveEmptyEntries));

        private string Serialize() =>
            string.Join(",", [.. shapeKeys]);
    }
}
