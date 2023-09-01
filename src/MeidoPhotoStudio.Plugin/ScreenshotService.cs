using System;
using System.Collections;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class ScreenshotService
{
    private readonly MeidoPhotoStudio meidoPhotoStudio;

    private bool takingScreenshot;

    public ScreenshotService(MeidoPhotoStudio meidoPhotoStudio) =>
        this.meidoPhotoStudio = meidoPhotoStudio;

    public void TakeScreenshotToFile(CMSystem.SSSuperSizeType? superSizeType = null)
    {
        if (!MeidoPhotoStudio.Instance)
            return;

        if (takingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        MeidoPhotoStudio.Instance.StartCoroutine(DoScreenshot(ScreenshotAction));

        void ScreenshotAction()
        {
            superSizeType ??= GameMain.Instance.CMSystem.ScreenShotSuperSize;

            var superSize = superSizeType switch
            {
                CMSystem.SSSuperSizeType.X1 => 1,
                CMSystem.SSSuperSizeType.X2 => 2,
                CMSystem.SSSuperSizeType.X4 => 4,

                // This value is never used in game so 8 is just a guess for what MAX could represent
                CMSystem.SSSuperSizeType.MAX => 8,
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

    public void TakeScreenshotToTexture(Action<Texture2D> screenshotCallback)
    {
        if (!MeidoPhotoStudio.Instance)
            return;

        if (takingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        if (screenshotCallback is null)
            throw new ArgumentNullException(nameof(screenshotCallback), "Screenshot callback is null");

        MeidoPhotoStudio.Instance.StartCoroutine(DoScreenshot(ScreenshotAction));

        void ScreenshotAction() =>
            screenshotCallback?.Invoke(TakeInMemoryScreenshot());

        Texture2D TakeInMemoryScreenshot()
        {
            var renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            RenderTexture.active = renderTexture;

            var mainCamera = GameMain.Instance.MainCamera;

            mainCamera.camera.targetTexture = renderTexture;
            mainCamera.camera.Render();

            var screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new(0f, 0f, renderTexture.width, renderTexture.height), 0, 0, false);
            screenshot.Apply();

            mainCamera.camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(renderTexture);

            return screenshot;
        }
    }

    private IEnumerator DoScreenshot(Action screenshotAction)
    {
        takingScreenshot = true;

        var uiCameras = UICamera.list.ToArray().Select(uiCamera => uiCamera.gameObject).ToArray();
        var dragHandleParent = GameObject.Find("[MPS DragPoint Parent]");

        SetElementsVisible(false);

        yield return new WaitForEndOfFrame();

        screenshotAction?.Invoke();

        PlayScreenshotSound();

        yield return new WaitForEndOfFrame();

        SetElementsVisible(true);

        takingScreenshot = false;

        void SetElementsVisible(bool visible)
        {
            foreach (var uiCamera in uiCameras)
                uiCamera.SetActive(visible);

            GizmoRender.UIVisible = visible;

            if (dragHandleParent)
                dragHandleParent.SetActive(visible);

            meidoPhotoStudio.UIActive = visible;
        }

        void PlayScreenshotSound() =>
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);
    }
}
