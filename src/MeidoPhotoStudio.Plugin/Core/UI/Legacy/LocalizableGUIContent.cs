using MeidoPhotoStudio.Plugin.Core.Localization;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class LocalizableGUIContent : GUIContent
{
    private readonly Translation translation;
    private readonly string category;
    private readonly string key;
    private readonly Func<string, string> formatter;

    public LocalizableGUIContent(Translation translation, string category, string key, Func<string, string> formatter = null)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.formatter = formatter;

        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        this.category = category;
        this.key = key;

        this.translation.Initialized += OnTranslationInitialized;

        text = FormattedText;
    }

    private string FormattedText =>
        formatter?.Invoke(translation[category, key]) ?? translation[category, key];

    public void Reformat() =>
        text = FormattedText;

    private void OnTranslationInitialized(object sender, EventArgs e) =>
        text = FormattedText;
}
