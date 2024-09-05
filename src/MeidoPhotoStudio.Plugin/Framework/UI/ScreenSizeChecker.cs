namespace MeidoPhotoStudio.Plugin.Framework.UI;

internal class ScreenSizeChecker : MonoBehaviour
{
    private Vector2 screenDimensions = new(Screen.width, Screen.height);

    public static event EventHandler ScreenSizeChanged;

    private void Update() =>
        CheckScreenDimensions();

    private void Start() =>
        CheckScreenDimensions();

    private void OnEnable() =>
        CheckScreenDimensions();

    private void CheckScreenDimensions()
    {
        var newScreenDimensions = new Vector2(Screen.width, Screen.height);

        if (newScreenDimensions == screenDimensions)
            return;

        screenDimensions = newScreenDimensions;

        ScreenSizeChanged?.Invoke(this, EventArgs.Empty);
    }
}
