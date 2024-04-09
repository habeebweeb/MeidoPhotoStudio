namespace MeidoPhotoStudio.Plugin.Core.Character;

public abstract class GravityController
{
    protected readonly CharacterController character;

    private GravityTransformControl transformControl;

    public GravityController(CharacterController character)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.character.ProcessingCharacterProps += OnCharacterProcessing;
    }

    public event EventHandler Moved;

    public event EventHandler EnabledChanged;

    public string Name =>
        $"{TypeName} Gravity Control ({character})";

    public bool Valid =>
        TransformControl ? TransformControl.isValid : false;

    public bool Enabled
    {
        get => TransformControl ? TransformControl.isEnabled : false;
        set
        {
            if (!TransformControl)
                return;

            TransformControl.isEnabled = value;

            EnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector3 Position
    {
        get => Transform.localPosition;
        set => SetPosition(value, true);
    }

    public Transform Transform =>
        TransformControl ? TransformControl.transform : null;

    protected abstract string TypeName { get; }

    protected GravityTransformControl TransformControl
    {
        get
        {
            if (character.Busy)
                return null;

            if (transformControl)
                return transformControl;

            transformControl = CreateTransformControl();

            return transformControl;
        }
    }

    public void SetPositionWithoutNotify(Vector3 position)
    {
        if (!Enabled)
            return;

        SetPosition(position, false);
    }

    public void OnControlMoved()
    {
        if (!Enabled)
            return;

        Moved?.Invoke(this, EventArgs.Empty);
    }

    protected abstract void InitializeTransformControl(GravityTransformControl transformControl);

    private void OnCharacterProcessing(object sender, CharacterProcessingEventArgs e)
    {
        var mpnStart = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.BODY_RELOAD_START)) - 1;
        var mpnEnd = (MPN)Enum.Parse(typeof(MPN_TYPE_RANGE), nameof(MPN_TYPE_RANGE.WEAR_END));

        if (!e.ChangingSlots.Any(slot => slot >= mpnStart || slot <= mpnEnd))
            return;

        Enabled = false;
        transformControl = null;
    }

    private void SetPosition(Vector3 position, bool notify)
    {
        if (!Enabled)
            return;

        Transform.localPosition = position;

        if (notify)
            OnControlMoved();
    }

    private GravityTransformControl CreateTransformControl()
    {
        var bone = character.GetBone("Bip01");
        var controlName = $"{TypeName} Gravity Control ({character.ID})";

        var findControlParent = character.Transform.Find(controlName);

        if (findControlParent)
            Object.Destroy(findControlParent.gameObject);

        var controlParent = new GameObject(controlName);

        controlParent.transform.SetParent(character.Transform);
        controlParent.transform.SetPositionAndRotation(bone.position, Quaternion.identity);
        controlParent.transform.localScale = Vector3.one;

        var controlGameObject = new GameObject(controlName);

        controlGameObject.transform.SetParent(controlParent.transform, false);

        var control = controlGameObject.AddComponent<GravityTransformControl>();

        InitializeTransformControl(control);

        return control;
    }
}
