namespace MeidoPhotoStudio.Plugin;

public class SceneManagementPane : BasePane
{
    private readonly Button sceneManagerButton;

    private string sceneManagementHeader;

    public SceneManagementPane(SceneWindow sceneWindow)
    {
        _ = sceneWindow ?? throw new System.ArgumentNullException(nameof(sceneWindow));

        sceneManagementHeader = Translation.Get("sceneManagementPane", "sceneManagementHeader");

        sceneManagerButton = new(Translation.Get("sceneManagementPane", "manageScenesButton"));
        sceneManagerButton.ControlEvent += (_, _) =>
            sceneWindow.Visible = !sceneWindow.Visible;
    }

    public override void Draw()
    {
        MpsGui.Header(sceneManagementHeader);
        MpsGui.WhiteLine();

        sceneManagerButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        sceneManagementHeader = Translation.Get("sceneManagementPane", "sceneManagementHeader");
        sceneManagerButton.Label = Translation.Get("backgroundWindow", "manageScenesButton");
    }
}
