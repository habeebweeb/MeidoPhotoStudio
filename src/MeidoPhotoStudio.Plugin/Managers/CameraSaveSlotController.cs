using MeidoPhotoStudio.Plugin.Framework.Collections;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class CameraSaveSlotController : IEnumerable<CameraInfo>
{
    private readonly CameraInfo defaultCameraInfo = new(new(0f, 0.9f, 0f), Quaternion.Euler(10f, 180f, 0f), 3f, 35f);
    private readonly CameraController cameraController;
    private SelectList<CameraInfo> cameraInfoSelectList;
    private CameraInfo temporaryCameraInfo;

    public CameraSaveSlotController(CameraController cameraController)
    {
        this.cameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));

        cameraInfoSelectList = new(new CameraInfo[SaveSlotCount]);
    }

    public int SaveSlotCount =>
        5;

    public int CurrentCameraSlot
    {
        get => cameraInfoSelectList.CurrentIndex;
        set
        {
            if ((uint)value >= cameraInfoSelectList.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            cameraInfoSelectList.Current = MainCamera.GetCameraInfo();
            cameraInfoSelectList.CurrentIndex = value;
            LoadCameraSlot(value);
        }
    }

    private static CameraMain MainCamera =>
        GameMain.Instance.MainCamera;

    public CameraInfo this[int index]
    {
        get =>
            (uint)index >= cameraInfoSelectList.Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : cameraInfoSelectList[index];

        set
        {
            if ((uint)index >= cameraInfoSelectList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            cameraInfoSelectList[index] = value;

            if (index == CurrentCameraSlot)
                cameraController.ApplyCameraInfo(value);
        }
    }

    public void Activate()
    {
        temporaryCameraInfo = defaultCameraInfo;
        cameraInfoSelectList = new(Enumerable.Repeat(defaultCameraInfo, cameraInfoSelectList.Count).ToArray());
    }

    public void LoadCameraSlot(int index)
    {
        if ((uint)index >= cameraInfoSelectList.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        cameraController.ApplyCameraInfo(cameraInfoSelectList.Current);

        MainCamera.StopAll();
    }

    public void SaveTemporaryCameraInfo()
    {
        temporaryCameraInfo = MainCamera.GetCameraInfo();

        MainCamera.StopAll();
    }

    public void LoadTemporaryCameraInfo()
    {
        cameraController.ApplyCameraInfo(temporaryCameraInfo);

        MainCamera.StopAll();
    }

    public IEnumerator<CameraInfo> GetEnumerator()
    {
        cameraInfoSelectList.Current = MainCamera.GetCameraInfo();

        return ((IEnumerable<CameraInfo>)cameraInfoSelectList).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        cameraInfoSelectList.Current = MainCamera.GetCameraInfo();

        return ((IEnumerable)cameraInfoSelectList).GetEnumerator();
    }
}
