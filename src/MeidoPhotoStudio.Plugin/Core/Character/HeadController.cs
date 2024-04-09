namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HeadController
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

    public bool FreeLook
    {
        get => Body.trsLookTarget == null;
        set => Body.trsLookTarget = value ? null : GameMain.Instance.MainCamera.transform;
    }

    public Vector2 OffsetLookTarget
    {
        get => new(Body.offsetLookTarget.z, Body.offsetLookTarget.x);
        set => Body.offsetLookTarget = new(value.y, 1f, value.x);
    }

    public bool HeadToCamera
    {
        get => Body.boHeadToCam;
        set
        {
            Body.HeadToCamPer = 0f;
            Body.HeadToCamFadeSpeed = 100f;
            Body.boHeadToCam = value;

            if (!HeadToCamera && !EyeToCamera)
                FreeLook = false;
        }
    }

    public bool EyeToCamera
    {
        get => Body.boEyeToCam;
        set
        {
            Body.boEyeToCam = value;

            if (!HeadToCamera && !EyeToCamera)
                FreeLook = false;
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
}
