using MeidoPhotoStudio.Plugin.Core;

using ScreenshotScale = CMSystem.SSSuperSizeType;

namespace MeidoPhotoStudio.Plugin.Framework.Service;

public class ScreenshotService : MonoBehaviour
{
    private GameObject dragHandleParent;

    public PluginCore PluginCore { get; set; }

    public bool TakingScreenshot { get; set; }

    private GameObject DragHandleParent =>
        dragHandleParent ? dragHandleParent : (dragHandleParent = GameObject.Find("[MPS Drag Handle Parent]"));

    public void TakeScreenshotToFile(ScreenshotOptions screenshotOptions)
    {
        if (TakingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        StartCoroutine(DoScreenshot(ScreenshotAction, screenshotOptions));

        static void ScreenshotAction()
        {
            var superSizeType = GameMain.Instance.CMSystem.ScreenShotSuperSize;

            var superSize = superSizeType switch
            {
                ScreenshotScale.X1 => 1,
                ScreenshotScale.X2 => 2,
                ScreenshotScale.X4 => 4,

                // This value is never used in game so 8 is just a guess for what MAX could represent
                ScreenshotScale.MAX => 8,
                _ => 1,
            };

            var screenshotDirectory = Path.Combine(GameMain.Instance.SerializeStorageManager.StoreDirectoryPath, "ScreenShot");

            if (!Directory.Exists(screenshotDirectory))
                Directory.CreateDirectory(screenshotDirectory);

            var screenshotFilename = $"img{DateTime.Now:yyyyMMddHHmmss}.png";
            var screenshotPath = Path.Combine(screenshotDirectory, screenshotFilename);

            Application.CaptureScreenshot(screenshotPath, superSize);
        }
    }

    public void TakeScreenshotToTexture(Action<Texture2D> screenshotCallback, ScreenshotOptions screenshotOptions)
    {
        if (TakingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        if (screenshotCallback is null)
            throw new ArgumentNullException(nameof(screenshotCallback), "Screenshot callback is null");

        StartCoroutine(DoScreenshot(ScreenshotAction, screenshotOptions));

        void ScreenshotAction() =>
            screenshotCallback?.Invoke(TakeInMemoryScreenshot(screenshotOptions));

        Texture2D TakeInMemoryScreenshot(ScreenshotOptions screenshotOptions)
        {
            if (screenshotOptions.CaptureMessageBox)
                return TakeScreenshot();

            var renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

            RenderTexture.active = renderTexture;

            var mainCamera = GameMain.Instance.MainCamera;

            mainCamera.camera.targetTexture = renderTexture;
            mainCamera.camera.Render();

            var screenshot = TakeScreenshot();

            mainCamera.camera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(renderTexture);

            return screenshot;

            static Texture2D TakeScreenshot()
            {
                var screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

                screenshot.ReadPixels(new(0f, 0f, Screen.width, Screen.height), 0, 0, false);
                screenshot.Apply();

                return screenshot;
            }
        }
    }

    private IEnumerator DoScreenshot(Action screenshotAction, ScreenshotOptions screenshotOptions)
    {
        TakingScreenshot = true;

        var uiPanels = GetNguiUIPanels();
        var enabledCanvases = GetEnabledUguiCanvases();

        SetElementsVisible(screenshotOptions.CaptureUI);

        yield return new WaitForEndOfFrame();

        try
        {
            screenshotAction?.Invoke();
        }
        catch
        {
            SetElementsVisible(true);

            TakingScreenshot = false;

            throw;
        }

        PlayScreenshotSound();

        yield return new WaitForEndOfFrame();

        SetElementsVisible(true);

        TakingScreenshot = false;

        void SetElementsVisible(bool visible)
        {
            foreach (var uiPanel in uiPanels)
                uiPanel.alpha = visible ? 1f : 0f;

            foreach (var canvas in enabledCanvases)
                canvas.enabled = visible;

            GizmoRender.UIVisible = visible;

            if (PluginCore)
                PluginCore.UIActive = visible;

            if (DragHandleParent)
                DragHandleParent.SetActive(visible);
        }

        Canvas[] GetEnabledUguiCanvases() =>
            Resources.FindObjectsOfTypeAll<Canvas>().Where(canvas => canvas.enabled).ToArray();

        UIPanel[] GetNguiUIPanels() =>
            NGUITools.FindActive<UICamera>()
                .Where(uiCamera => uiCamera.cachedCamera.enabled)
                .Select(uiCamera => NGUITools.FindInParents<UIRoot>(uiCamera.gameObject))
                .Where(uiRoot => uiRoot)
                .SelectMany(uiRoot => uiRoot.transform.Cast<Transform>())
                .Select(transform => transform.GetComponent<UIPanel>())
                .Where(uiPanel => uiPanel && uiPanel.alpha is not 0f && uiPanel.name is not "MessageWindowPanel")
                .ToArray();

        void PlayScreenshotSound() =>
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);
    }
}
