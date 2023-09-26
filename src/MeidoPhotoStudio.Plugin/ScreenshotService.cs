using System;
using System.Collections;
using System.IO;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Screenshot service.</summary>
public partial class ScreenshotService : MonoBehaviour
{
    private bool takingScreenshot;
    private GameObject dragHandleParent;

    public PluginCore PluginCore { get; set; }

    private GameObject DragHandleParent =>
        dragHandleParent ? dragHandleParent : (dragHandleParent = GameObject.Find("[MPS DragPoint Parent]"));

    public void TakeScreenshotToFile(CMSystem.SSSuperSizeType? superSizeType = null)
    {
        if (takingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        StartCoroutine(DoScreenshot(ScreenshotAction));

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
        if (takingScreenshot)
        {
            Utility.LogInfo("Screenshot in progress");

            return;
        }

        if (screenshotCallback is null)
            throw new ArgumentNullException(nameof(screenshotCallback), "Screenshot callback is null");

        StartCoroutine(DoScreenshot(ScreenshotAction));

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
            DestroyImmediate(renderTexture);

            return screenshot;
        }
    }

    private IEnumerator DoScreenshot(Action screenshotAction)
    {
        takingScreenshot = true;

        var uiCameras = GetNguiUICameras();
        var enabledCanvases = GetEnabledUguiCanvases();

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

        GameObject[] GetNguiUICameras() =>
            UICamera.list.ToArray().Select(uiCamera => uiCamera.gameObject).ToArray();

        void PlayScreenshotSound() =>
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);
    }
}
