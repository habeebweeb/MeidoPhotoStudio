using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.StartupPreset;
using MeidoPhotoStudio.Plugin.Framework.Collections;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class StartupPresetSettingsPane : BasePane
{
    private readonly Translation translation;
    private readonly StartupPresetConfiguration startupPresetConfiguration;
    private readonly StartupPresetService startupPresetService;
    private readonly Toggle enabledToggle;
    private readonly Header customPresetHeader;
    private readonly Toggle useCustomPresetToggle;
    private readonly Button refreshStartupPresetButton;
    private readonly Button updateStartupPreset;
    private readonly GUIContent presetThumbnailContent;
    private readonly Header loadOptionsHeader;
    private readonly Label loadOptionsExplanationLabel;
    private readonly Tree<Toggle> loadOptionToggles;
    private readonly Dictionary<int, LazyStyle> loadOptionToggleStyles = [];

    private readonly LazyStyle thumbnailStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            margin = new(0, 0, 0, 0),
            border = new(0, 0, 0, 0),
            normal = { background = Texture2D.whiteTexture },
            stretchWidth = false,
            stretchHeight = false,
        });

    public StartupPresetSettingsPane(
        Translation translation,
        StartupPresetConfiguration startupPresetConfiguration,
        StartupPresetService startupPresetService)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.startupPresetConfiguration = startupPresetConfiguration
            ?? throw new ArgumentNullException(nameof(startupPresetConfiguration));

        this.startupPresetService = startupPresetService
            ?? throw new ArgumentNullException(nameof(startupPresetService));

        this.startupPresetService.UpdatedCustomStartupPreset += OnCustomStartupPresetUpdated;
        this.startupPresetService.RefreshedCustomStartupPreset += OnCustomStartupPresetRefreshed;
        this.startupPresetService.LoadOptions.AddedOption += OnLoadOptionAdded;

        enabledToggle = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "enabledToggle"),
            this.startupPresetConfiguration.Enabled.Value);

        enabledToggle.ControlEvent += OnEnabledToggleChanged;

        customPresetHeader = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "customPresetHeader"));

        useCustomPresetToggle = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "useCustomPresetToggle"),
            this.startupPresetConfiguration.UseCustomPreset.Value);

        useCustomPresetToggle.ControlEvent += OnUseCustomPresetToggleChanged;

        updateStartupPreset = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "updateStartupPresetButton"));

        updateStartupPreset.ControlEvent += OnUpdateStartupPresetButtonPushed;

        refreshStartupPresetButton = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "refreshStartupPresetButton"));

        refreshStartupPresetButton.ControlEvent += OnRefreshStartupPresetButtonPushed;

        loadOptionsHeader = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "loadOptionsHeader"));

        loadOptionsExplanationLabel = new(
            new LocalizableGUIContent(translation, "startupPresetSettingsPane", "loadOptionsExplanationLabel"));

        var ignoredOptions = new HashSet<string>()
        {
            "characters",
            "message",
        };

        foreach (var loadOption in this.startupPresetService.LoadOptions
            .Where(option => !ignoredOptions.Contains(option.Tag)))
            AddLoadOptionConfiguration(loadOption);

        loadOptionToggles = new([..
            this.startupPresetService.LoadOptions
                .Where(option => !ignoredOptions.Contains(option.Tag))
                .Select(CreateLoadOptionToggles)]);

        presetThumbnailContent = new();

        UpdateCustomPresetThumbnail();
    }

    public override void Draw()
    {
        enabledToggle.Draw();

        if (!startupPresetConfiguration.Enabled.Value)
            return;

        UIUtility.DrawBlackLine();

        loadOptionsHeader.Draw();
        loadOptionsExplanationLabel.Draw();

        foreach (var loadOptionToggle in loadOptionToggles)
            DrawLoadOption(loadOptionToggle);

        UIUtility.DrawBlackLine();

        customPresetHeader.Draw();

        GUILayout.BeginHorizontal();

        useCustomPresetToggle.Draw();

        GUI.enabled = Parent.Enabled && startupPresetConfiguration.UseCustomPreset.Value;

        updateStartupPreset.Draw(GUILayout.ExpandWidth(false));
        refreshStartupPresetButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        if (presetThumbnailContent.image)
        {
            var thumbnail = presetThumbnailContent.image;

            var windowRect = Parent.WindowRect;

            var windowWidth = windowRect.width - UIUtility.Scaled(200) - 35;
            var windowHeight = windowRect.height;

            var scaledWidth = (ScaledMinimum(windowWidth) - 35) / (float)thumbnail.width;
            var scaledHeight = ScaledMinimum(windowHeight) / (float)thumbnail.height;

            var scale = Mathf.Min(scaledWidth, scaledHeight);

            var thumbnailWidth = Mathf.Min(thumbnail.width * scale, thumbnail.width);
            var thumbnailHeight = Mathf.Min(thumbnail.height * scale, thumbnail.height);

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.Box(
                presetThumbnailContent,
                thumbnailStyle,
                GUILayout.Width(thumbnailWidth),
                GUILayout.Height(thumbnailHeight));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        GUI.enabled = Parent.Enabled;

        static int ScaledMinimum(float value) =>
            Mathf.Min(UIUtility.Scaled(Mathf.RoundToInt(value)), (int)value);

        void DrawLoadOption(Tree<Toggle>.Node toggleNode, int depth = 0)
        {
            if (depth is 0)
            {
                toggleNode.Value.Draw();
            }
            else
            {
                if (!loadOptionToggleStyles.TryGetValue(depth, out var style))
                    loadOptionToggleStyles[depth] = style = new(
                        StyleSheet.TextSize,
                        () => new(GUI.skin.toggle)
                        {
                            margin = { left = UIUtility.Scaled(15 * depth) },
                        },
                        style => style.margin.left = UIUtility.Scaled(15 * depth));

                toggleNode.Value.Draw(style);
            }

            if (!toggleNode.Value.Value)
                return;

            foreach (var subLoadOption in toggleNode)
                DrawLoadOption(subLoadOption, depth + 1);
        }
    }

    private Tree<Toggle>.Node CreateLoadOptionToggles(LoadOption loadOption)
    {
        const string LoadOptionTableKey = "baseLoadOptions";

        var content = translation.ContainsTranslation(LoadOptionTableKey, loadOption.Path)
            ? new LocalizableGUIContent(translation, LoadOptionTableKey, loadOption.Path)
            : new GUIContent(loadOption.Tag);

        var toggle = new Toggle(content, loadOption.Enabled);
        var entry = startupPresetConfiguration.GetLoadOptionEntry(loadOption);

        toggle.ControlEvent += (_, _) =>
            loadOption.Enabled = entry.Value = toggle.Value;

        entry.SettingChanged += (_, _) =>
            toggle.SetEnabledWithoutNotify(entry.Value);

        return new(toggle, [.. loadOption.Select(CreateLoadOptionToggles)]);
    }

    private void AddLoadOptionConfiguration(LoadOption loadOption)
    {
        var entry = startupPresetConfiguration.GetLoadOptionEntry(loadOption);

        loadOption.Enabled = entry.Value;

        foreach (var child in loadOption)
            AddLoadOptionConfiguration(child);
    }

    private void OnCustomStartupPresetUpdated(object sender, EventArgs e) =>
        UpdateCustomPresetThumbnail();

    private void OnCustomStartupPresetRefreshed(object sender, EventArgs e) =>
        UpdateCustomPresetThumbnail();

    private void OnLoadOptionAdded(object sender, LoadOptionsChangedEventArgs e)
    {
        AddLoadOptionConfiguration(e.LoadOption);
        loadOptionToggles.Add(CreateLoadOptionToggles(e.LoadOption));
    }

    private void OnEnabledToggleChanged(object sender, EventArgs e) =>
        startupPresetService.Enabled = startupPresetConfiguration.Enabled.Value = enabledToggle.Value;

    private void OnUseCustomPresetToggleChanged(object sender, EventArgs e) =>
         startupPresetService.UseCustomPreset = startupPresetConfiguration.UseCustomPreset.Value = useCustomPresetToggle.Value;

    private void OnUpdateStartupPresetButtonPushed(object sender, EventArgs e)
    {
        startupPresetService.UpdateStartupPreset();

        UpdateCustomPresetThumbnail();
    }

    private void OnRefreshStartupPresetButtonPushed(object sender, EventArgs e) =>
        startupPresetService.RefreshStartupPreset();

    private void UpdateCustomPresetThumbnail()
    {
        Texture2D newThumbnail = null;

        try
        {
            newThumbnail = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            newThumbnail.LoadImage(File.ReadAllBytes(startupPresetService.StartupPresetPath));

            if (presetThumbnailContent.image)
                Object.DestroyImmediate(presetThumbnailContent.image);

            presetThumbnailContent.image = newThumbnail;
        }
        catch
        {
            if (newThumbnail)
                Object.DestroyImmediate(newThumbnail);
        }
    }
}
