using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HeadController : INotifyPropertyChanged
{
    private readonly CharacterController character;
    private readonly Quaternion initialLeftEyeRotation;
    private readonly Quaternion initialRightEyeRotation;

    public HeadController(CharacterController characterController)
    {
        character = characterController ?? throw new ArgumentNullException(nameof(characterController));

        initialLeftEyeRotation = character.Maid.body0.quaDefEyeL;
        initialRightEyeRotation = character.Maid.body0.quaDefEyeR;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool FreeLook
    {
        get => Body.trsLookTarget == null;
        set
        {
            if (FreeLook == value)
                return;

            Body.trsLookTarget = value ? null : GameMain.Instance.MainCamera.transform;

            RaisePropertyChanged(nameof(FreeLook));
        }
    }

    public Vector2 OffsetLookTarget
    {
        get => new(Body.offsetLookTarget.z, Body.offsetLookTarget.x);
        set
        {
            if (OffsetLookTarget == value)
                return;

            Body.offsetLookTarget = new(value.y, 1f, value.x);

            RaisePropertyChanged(nameof(OffsetLookTarget));
        }
    }

    public bool HeadToCamera
    {
        get => Body.boHeadToCam;
        set
        {
            if (HeadToCamera == value)
                return;

            Body.HeadToCamPer = 0f;
            Body.HeadToCamFadeSpeed = 100f;
            Body.boHeadToCam = value;

            if (!HeadToCamera && !EyeToCamera)
                FreeLook = false;

            RaisePropertyChanged(nameof(HeadToCamera));
        }
    }

    public bool EyeToCamera
    {
        get => Body.boEyeToCam;
        set
        {
            if (EyeToCamera == value)
                return;

            Body.boEyeToCam = value;

            if (!HeadToCamera && !EyeToCamera)
                FreeLook = false;

            RaisePropertyChanged(nameof(EyeToCamera));
        }
    }

    internal Quaternion LeftEyeRotation
    {
        get => Body.quaDefEyeL;
        set => Body.quaDefEyeL = value;
    }

    internal Quaternion RightEyeRotation
    {
        get => Body.quaDefEyeR;
        set => Body.quaDefEyeR = value;
    }

    internal Vector3 HeadRotation
    {
        get => Body.HeadEulerAngle;
        set
        {
            Body.HeadEulerAngleG = Vector3.zero;
            Body.HeadEulerAngle = value;
        }
    }

    internal Quaternion LeftEyeRotationDelta
    {
        get => Body.quaDefEyeL * Quaternion.Inverse(initialLeftEyeRotation);
        set => Body.quaDefEyeL = value * initialLeftEyeRotation;
    }

    internal Quaternion RightEyeRotationDelta
    {
        get => Body.quaDefEyeR * Quaternion.Inverse(initialRightEyeRotation);
        set => Body.quaDefEyeR = value * initialRightEyeRotation;
    }

    private Maid Maid =>
        character.Maid;

    private TBody Body =>
        Maid.body0;

    public void ResetBothEyeRotations()
    {
        ResetLeftEyeRotation();
        ResetRightEyeRotation();
    }

    public void RotateLeftEye(float x, float y)
    {
        var horizontalRotation = new Vector3(0f, -x, 0f);
        var verticalRotation = new Vector3(0f, 0f, -y);

        Body.quaDefEyeL = Quaternion.Euler(horizontalRotation) * Body.quaDefEyeL;
        Body.quaDefEyeL *= Quaternion.Euler(verticalRotation);
    }

    public void RotateRightEye(float x, float y)
    {
        var horizontalRotation = new Vector3(0f, x, 0f);
        var verticalRotation = new Vector3(0f, 0f, y);

        Body.quaDefEyeR = Quaternion.Euler(horizontalRotation) * Body.quaDefEyeR;
        Body.quaDefEyeR *= Quaternion.Euler(verticalRotation);
    }

    public void RotateBothEyes(float x, float y)
    {
        RotateLeftEye(x, y);
        RotateRightEye(x, y);
    }

    public void ResetLeftEyeRotation() =>
        Body.quaDefEyeL = initialLeftEyeRotation;

    public void ResetRightEyeRotation() =>
        Body.quaDefEyeR = initialRightEyeRotation;

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
