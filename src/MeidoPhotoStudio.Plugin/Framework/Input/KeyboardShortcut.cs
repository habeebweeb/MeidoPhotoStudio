using BepInEx.Configuration;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.Input;

/// <summary>BepInEx's KeyboardShortcut turned into a class for polymorphism.</summary>
public class KeyboardShortcut : KeyboardInput
{
    public static readonly KeyboardShortcut Empty = new();

    static KeyboardShortcut() =>
        TomlTypeConverter.AddConverter(
            typeof(KeyboardShortcut),
            new()
            {
                ConvertToString = (shortcut, _) => ((KeyboardShortcut)shortcut).Serialize(),
                ConvertToObject = (data, _) => Deserialize(data),
            });

    public KeyboardShortcut(KeyCode mainKey, params KeyCode[] modifiers)
        : base(SanitizeKeys([mainKey, .. modifiers]))
    {
        if (mainKey is KeyCode.None && modifiers.Any())
            throw new ArgumentException(
                $"Can't set {nameof(mainKey)} to KeyCode.None if there are any {nameof(modifiers)}");
    }

    private KeyboardShortcut(params KeyCode[] keys)
        : base(SanitizeKeys(keys))
    {
    }

    public KeyCode MainKey =>
        Keys?.Length > 0 ? Keys[0] : KeyCode.None;

    public IEnumerable<KeyCode> Modifiers =>
        Keys?.Skip(1) ?? Enumerable.Empty<KeyCode>();

    public override bool IsDown()
    {
        var mainKey = MainKey;

        return mainKey is not KeyCode.None && UInput.GetKeyDown(mainKey) && AllModifiersPressed()
            && NoOtherKeysPressed();
    }

    public override bool IsPressed()
    {
        var mainKey = MainKey;

        return mainKey is not KeyCode.None && UInput.GetKey(mainKey) && AllModifiersPressed() && NoOtherKeysPressed();
    }

    public override bool IsUp()
    {
        var mainKey = MainKey;

        return mainKey is not KeyCode.None && UInput.GetKeyUp(mainKey) && AllModifiersPressed() && NoOtherKeysPressed();
    }

    public override bool Equals(object obj) =>
        obj is KeyboardShortcut shortcut && MainKey == shortcut.MainKey && Modifiers.SequenceEqual(shortcut.Modifiers);

    public override int GetHashCode() =>
        MainKey is KeyCode.None ? 0 : base.GetHashCode();

    private static KeyboardShortcut Deserialize(string data)
    {
        try
        {
            return new(DeserializeKeyCodes(data));
        }
        catch
        {
            return Empty;
        }
    }

    private static KeyCode[] SanitizeKeys(params KeyCode[] keys) =>
        keys.Length is 0 || keys.Any(key => key is KeyCode.None)
            ? [KeyCode.None]
            : [keys[0], .. keys.Skip(1).Distinct().Where(key => key != keys[0]).OrderBy(key => (int)key)];

    private bool AllModifiersPressed()
    {
        var mainKey = MainKey;

        return Keys.All(keyCode => keyCode == mainKey || UInput.GetKey(keyCode));
    }
}
