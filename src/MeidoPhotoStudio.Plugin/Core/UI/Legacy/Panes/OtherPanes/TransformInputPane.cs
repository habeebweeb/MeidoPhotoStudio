using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using TransformType = MeidoPhotoStudio.Plugin.Framework.Service.TransformClipboard.TransformType;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TransformInputPane : BasePane
{
    private readonly TransformControl positionControl;
    private readonly TransformControl rotationControl;
    private readonly TransformControl scaleControl;

    private bool internalChange;
    private IObservableTransform target;

    public TransformInputPane(Translation translation, TransformClipboard transformClipboard)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        _ = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        positionControl = new(translation, transformClipboard, TransformType.Position);
        positionControl.ControlEvent += OnPositionChanged;
        positionControl.Transforming += OnTransforming;
        positionControl.Transformed += OnTransformed;
        positionControl.CancelledTransformation += OnCancelledTransformation;

        rotationControl = new(translation, transformClipboard, TransformType.Rotation);
        rotationControl.ControlEvent += OnRotationChanged;
        rotationControl.Transforming += OnTransforming;
        rotationControl.Transformed += OnTransformed;
        rotationControl.CancelledTransformation += OnCancelledTransformation;

        scaleControl = new(translation, transformClipboard, TransformType.Scale);
        scaleControl.ControlEvent += OnScaleChanged;
        scaleControl.Transforming += OnTransforming;
        scaleControl.Transformed += OnTransformed;
        scaleControl.CancelledTransformation += OnCancelledTransformation;
    }

    public event EventHandler Transforming;

    public event EventHandler Transformed;

    public event EventHandler CancelledTransformation;

    public Space Space { get; set; } = Space.World;

    public bool EnablePosition { get; set; } = true;

    public bool EnableRotation { get; set; } = true;

    public bool EnableScale { get; set; } = true;

    public bool LinkScale
    {
        get => scaleControl.LinkFields;
        set => scaleControl.LinkFields = value;
    }

    public IObservableTransform Target
    {
        get => target;
        set
        {
            if (target is not null)
                target.ChangedTransform -= OnTransformChanged;

            target = value;

            if (target is not null)
                target.ChangedTransform += OnTransformChanged;

            positionControl.SetValueWithoutNotify(Position);
            rotationControl.SetValueWithoutNotify(Rotation);
            scaleControl.SetValueWithoutNotify(Scale);

            var initialTransform = target?.InitialTransform ?? new(Space, Vector3.zero, Quaternion.identity, Vector3.one);

            positionControl.DefaultValue = initialTransform.Position;
            rotationControl.DefaultValue = initialTransform.Rotation.eulerAngles;
            scaleControl.DefaultValue = initialTransform.LocalScale;
        }
    }

    public Vector3 Position =>
        !Transform ? Vector3.zero :
        Space is Space.Self ? Transform.localPosition :
        Transform.position;

    public Vector3 Rotation =>
        !Transform ? Vector3.zero :
        Space is Space.Self ? Transform.localEulerAngles :
        Transform.eulerAngles;

    public Vector3 Scale =>
        !Transform ? Vector3.one : Transform.localScale;

    private Transform Transform =>
        Target.Transform;

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled && Target is not null;

        var fieldWidth = GUILayout.Width((Parent.WindowRect.width - 23f * 3 - 18f) / 3f);

        if (EnablePosition)
            positionControl.Draw(fieldWidth);

        if (EnableRotation)
            rotationControl.Draw(fieldWidth);

        if (EnableScale)
            scaleControl.Draw(fieldWidth);
    }

    private void OnTransformChanged(object sender, EventArgs e)
    {
        if (internalChange)
        {
            internalChange = false;

            return;
        }

        positionControl.SetValueWithoutNotify(Position);
        rotationControl.SetValueWithoutNotify(Rotation);
        scaleControl.SetValueWithoutNotify(Scale);
    }

    private void OnPositionChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        internalChange = true;

        if (Space is Space.Self)
            Transform.localPosition = positionControl.Value;
        else
            Transform.position = positionControl.Value;
    }

    private void OnRotationChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        internalChange = true;

        if (Space is Space.Self)
            Transform.localEulerAngles = rotationControl.Value;
        else
            Transform.eulerAngles = rotationControl.Value;
    }

    private void OnScaleChanged(object sender, EventArgs e)
    {
        if (!Transform)
            return;

        var value = scaleControl.Value;

        if (value.x < 0f || value.y < 0f || value.z < 0f)
            return;

        internalChange = true;

        Transform.localScale = scaleControl.Value;
    }

    private void OnTransforming(object sender, EventArgs e) =>
        Transforming?.Invoke(this, EventArgs.Empty);

    private void OnTransformed(object sender, EventArgs e) =>
        Transformed?.Invoke(this, EventArgs.Empty);

    private void OnCancelledTransformation(object sender, EventArgs e) =>
        CancelledTransformation?.Invoke(this, EventArgs.Empty);

    private class TransformControl : BaseControl
    {
        private static readonly LazyStyle HeaderStyle = new(
            StyleSheet.SubHeadingSize,
            static () => new(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
            });

        private static readonly LazyStyle LabelStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.label));
        private static readonly GUIContent XLabelContent = new("X");
        private static readonly GUIContent YLabelContent = new("Y");
        private static readonly GUIContent ZLabelContent = new("Z");

        private readonly TransformClipboard clipboard;
        private readonly TransformType transformType;
        private readonly float deltaScaling;
        private readonly Button copyButton;
        private readonly Button pasteButton;
        private readonly Button resetButton;
        private readonly NumericalTextField xTextField;
        private readonly NumericalTextField yTextField;
        private readonly NumericalTextField zTextField;
        private readonly Label header;

        private Vector3 initialValue;
        private float clickTime;
        private bool mouseDown;
        private bool xFocus;
        private bool yFocus;
        private bool zFocus;

        public TransformControl(Translation translation, TransformClipboard clipboard, TransformType transformType)
        {
            _ = translation ?? throw new ArgumentNullException(nameof(translation));
            this.clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
            this.transformType = transformType;

            var (tableKey, translationKey) = transformType switch
            {
                TransformType.Position => ("transformInputPane", "positionHeader"),
                TransformType.Rotation => ("transformInputPane", "rotationHeader"),
                TransformType.Scale => ("transformInputPane", "scaleHeader"),
                _ => throw new ArgumentOutOfRangeException(nameof(transformType)),
            };

            header = new(new LocalizableGUIContent(translation, tableKey, translationKey));

            copyButton = new(new LocalizableGUIContent(translation, "transformInputPane", "copyButton"));
            copyButton.ControlEvent += OnCopyButtonPushed;

            pasteButton = new(new LocalizableGUIContent(translation, "transformInputPane", "pasteButton"));
            pasteButton.ControlEvent += OnPasteButtonPushed;

            resetButton = new(new LocalizableGUIContent(translation, "transformInputPane", "resetButton"));
            resetButton.ControlEvent += OnResetButtonPushed;

            xTextField = new(DefaultValue.x);
            yTextField = new(DefaultValue.y);
            zTextField = new(DefaultValue.z);

            xTextField.ControlEvent += OnXTextFieldChanged;
            yTextField.ControlEvent += OnYTextFieldChanged;
            zTextField.ControlEvent += OnZTextFieldChanged;

            deltaScaling = this.transformType switch
            {
                TransformType.Position => 0.015f,
                TransformType.Rotation => 1.7f,
                TransformType.Scale => 0.015f,
                _ => 1f,
            };
        }

        public event EventHandler Transforming;

        public event EventHandler Transformed;

        public event EventHandler CancelledTransformation;

        public Vector3 DefaultValue { get; set; }

        public Vector3 Value
        {
            get => new(xTextField.Value, yTextField.Value, zTextField.Value);
            set => SetValue(value);
        }

        public bool LinkFields { get; set; }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            var noExpandWidth = GUILayout.ExpandWidth(false);

            GUILayout.BeginHorizontal();

            header.Draw(HeaderStyle);

            GUILayout.FlexibleSpace();

            if (clipboard is not null)
            {
                copyButton.Draw(noExpandWidth);
                pasteButton.Draw(noExpandWidth);
            }

            resetButton.Draw(noExpandWidth);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label(XLabelContent, LabelStyle);

            Rect xRect = default;

            if (!mouseDown)
                xRect = GUILayoutUtility.GetLastRect();

            xTextField.Draw(layoutOptions);

            GUILayout.Label(YLabelContent, LabelStyle);

            Rect yRect = default;

            if (!mouseDown)
                yRect = GUILayoutUtility.GetLastRect();

            yTextField.Draw(layoutOptions);

            GUILayout.Label(ZLabelContent, LabelStyle);

            Rect zRect = default;

            if (!mouseDown)
                zRect = GUILayoutUtility.GetLastRect();

            zTextField.Draw(layoutOptions);

            GUILayout.EndHorizontal();

            if (!mouseDown)
            {
                if (Event.current is { type: EventType.MouseDown } e)
                {
                    xFocus = xRect.Contains(e.mousePosition);
                    yFocus = yRect.Contains(e.mousePosition);
                    zFocus = zRect.Contains(e.mousePosition);

                    mouseDown = xFocus || yFocus || zFocus;

                    if (mouseDown)
                    {
                        if (Time.time - clickTime <= 0.3f)
                        {
                            if (xFocus)
                                xTextField.Value = DefaultValue.x;
                            else if (yFocus)
                                yTextField.Value = DefaultValue.y;
                            else if (zFocus)
                                zTextField.Value = DefaultValue.z;

                            mouseDown = false;
                        }

                        clickTime = Time.time;
                        initialValue = Value;
                        e.Use();
                        Transforming?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                if (Event.current is { type: EventType.MouseDown, button: 1 } e)
                {
                    Value = initialValue;
                    StopDrag();
                    CancelledTransformation?.Invoke(this, EventArgs.Empty);
                    e.Use();
                }
                else if (UnityEngine.Input.GetMouseButtonDown(1) && Event.current.type is EventType.Repaint)
                {
                    Value = initialValue;
                    CancelledTransformation?.Invoke(this, EventArgs.Empty);
                    StopDrag();
                }
                else if (!UnityEngine.Input.GetMouseButton(0))
                {
                    StopDrag();
                }
                else
                {
                    var delta = UnityEngine.Input.GetAxis("Mouse X") * deltaScaling;

                    if (UnityEngine.Input.GetKey(KeyCode.LeftControl))
                        delta *= 0.25f;

                    if (xFocus)
                        xTextField.Value += delta;
                    else if (yFocus)
                        yTextField.Value += delta;
                    else if (zFocus)
                        zTextField.Value += delta;
                }

                void StopDrag()
                {
                    Transformed?.Invoke(this, EventArgs.Empty);
                    mouseDown = false;
                    xFocus = false;
                    yFocus = false;
                    zFocus = false;
                }
            }
        }

        public void SetValueWithoutNotify(Vector3 value) =>
            SetValue(value, false);

        private void OnCopyButtonPushed(object sender, EventArgs e) =>
            clipboard[transformType] = Value;

        private void OnPasteButtonPushed(object sender, EventArgs e)
        {
            if (clipboard[transformType] is not Vector3 value)
                return;

            if (LinkFields)
            {
                var average = (value.x + value.y + value.z) / 3f;

                value = Vector3.one * average;
            }

            SetValue(value);
        }

        private void OnResetButtonPushed(object sender, EventArgs e) =>
            SetValue(DefaultValue);

        private void OnXTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * xTextField.Value : Value);

        private void OnYTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * yTextField.Value : Value);

        private void OnZTextFieldChanged(object sender, EventArgs e) =>
            SetValue(LinkFields ? Vector3.one * zTextField.Value : Value);

        private void SetValue(Vector3 value, bool notify = true)
        {
            xTextField.SetValueWithoutNotify(value.x);
            yTextField.SetValueWithoutNotify(value.y);
            zTextField.SetValueWithoutNotify(value.z);

            if (!notify)
                return;

            OnControlEvent(EventArgs.Empty);
        }
    }
}
