using MeidoPhotoStudio.Plugin.Core.Scenes;

namespace MeidoPhotoStudio.Plugin;

public class SceneManagementPane : BasePane
{
    private readonly SceneBrowserWindow sceneWindow;
    private readonly QuickSaveService quickSaveService;
    private readonly Button manageScenesButton;
    private readonly Button quickSaveButton;
    private readonly Button quickLoadButton;

    private string sceneManagementHeader;

    public SceneManagementPane(SceneBrowserWindow sceneWindow, QuickSaveService quickSaveService)
    {
        this.sceneWindow = sceneWindow ?? throw new ArgumentNullException(nameof(sceneWindow));
        this.quickSaveService = quickSaveService ?? throw new ArgumentNullException(nameof(quickSaveService));

        sceneManagementHeader = Translation.Get("sceneManagementPane", "sceneManagementHeader");

        manageScenesButton = new(Translation.Get("sceneManagementPane", "manageScenesButton"));
        manageScenesButton.ControlEvent += OnManageScenesButtonPushed;

        quickSaveButton = new(Translation.Get("sceneManagementPane", "quickSaveButton"));
        quickSaveButton.ControlEvent += OnQuickSaveButtonPushed;

        quickLoadButton = new(Translation.Get("sceneManagementPane", "quickLoadButton"));
        quickLoadButton.ControlEvent += OnQuickLoadButtonPushed;
    }

    public override void Draw()
    {
        MpsGui.Header(sceneManagementHeader);
        MpsGui.WhiteLine();

        manageScenesButton.Draw();
        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        quickSaveButton.Draw();
        quickLoadButton.Draw();

        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        sceneManagementHeader = Translation.Get("sceneManagementPane", "sceneManagementHeader");
        manageScenesButton.Label = Translation.Get("backgroundWindow", "manageScenesButton");
        quickSaveButton.Label = Translation.Get("sceneManagementPane", "quickSaveButton");
        quickLoadButton.Label = Translation.Get("sceneManagementPane", "quickLoadButton");
    }

    private void OnManageScenesButtonPushed(object sender, EventArgs e) =>
        sceneWindow.Visible = !sceneWindow.Visible;

    private void OnQuickSaveButtonPushed(object sender, EventArgs e) =>
        quickSaveService.QuickSave();

    private void OnQuickLoadButtonPushed(object sender, EventArgs e) =>
        quickSaveService.QuickLoad();
}
