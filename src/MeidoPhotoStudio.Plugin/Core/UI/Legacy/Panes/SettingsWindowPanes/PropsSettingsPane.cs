using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropsSettingsPane : BasePane
{
    private static readonly LazyStyle MenuRepositoryBusyLabelStyle = new(
        StyleSheet.TextSize,
        () => new(GUI.skin.label)
        {
            fontStyle = FontStyle.Italic,
        });

    private readonly PropsConfiguration configuration;
    private readonly MenuPropRepository menuPropRepository;
    private readonly Toggle ignoreGameMenuFilesToggle;
    private readonly Label ignoreGameMenuFilesInfoLabel;
    private readonly Label initialKeepWorldPositionLabel;
    private readonly Toggle.Group initialKeepWorldPositionToggleGroup;

    public PropsSettingsPane(
        Translation translation, PropsConfiguration configuration, MenuPropRepository menuPropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

        ignoreGameMenuFilesToggle = new(
            new LocalizableGUIContent(translation, "propsSettingsPane", "ignoreGameMenuFilesToggle"),
            this.configuration.IgnoreGameMenuFiles.Value);

        ignoreGameMenuFilesToggle.ControlEvent += OnIgnoreMenuFilesToggleChanged;

        ignoreGameMenuFilesInfoLabel = new(
            new LocalizableGUIContent(translation, "propsSettingsPane", "menuFileRepositoryBusyLabel"));

        initialKeepWorldPositionLabel = new(
            new LocalizableGUIContent(translation, "propsSettingsPane", "initialKeepWorldPositionLabel"));

        var onToggle = new Toggle(
            new LocalizableGUIContent(translation, "propsSettingsPane", "onToggle"),
            this.configuration.InitialKeepPositionOnAttachState.Value is true);

        onToggle.ControlEvent += (_, _) =>
            this.configuration.InitialKeepPositionOnAttachState.Value = true;

        var offToggle = new Toggle(
            new LocalizableGUIContent(translation, "propsSettingsPane", "offToggle"),
            this.configuration.InitialKeepPositionOnAttachState.Value is false);

        offToggle.ControlEvent += (_, _) =>
            this.configuration.InitialKeepPositionOnAttachState.Value = false;

        initialKeepWorldPositionToggleGroup = [onToggle, offToggle];
    }

    public override void Draw()
    {
        if (!menuPropRepository.Busy)
        {
            ignoreGameMenuFilesToggle.Draw();
        }
        else
        {
            GUI.enabled = false;

            ignoreGameMenuFilesToggle.Draw();

            GUI.enabled = Parent.Enabled;

            ignoreGameMenuFilesInfoLabel.Draw(MenuRepositoryBusyLabelStyle);
        }

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        initialKeepWorldPositionLabel.Draw();

        foreach (var toggle in initialKeepWorldPositionToggleGroup)
            toggle.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    private void OnIgnoreMenuFilesToggleChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        configuration.IgnoreGameMenuFiles.Value = ignoreGameMenuFilesToggle.Value;

        menuPropRepository.Refresh();
    }
}
