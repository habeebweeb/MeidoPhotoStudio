namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class RepeatButton : BaseControl
{
    private string label;
    private Texture icon;
    private GUIContent buttonContent;
    private float startClickTime;
    private float holdTime;
    private bool clicked;
    private float scaledInterval;
    private float interval;

    public RepeatButton(string label, float interval = 0f)
        : this(interval) =>
        Label = label;

    public RepeatButton(Texture icon, float interval = 0f)
        : this(interval) =>
        Icon = icon;

    protected RepeatButton(float interval) =>
        Interval = interval;

    public static LazyStyle Style { get; } = new(13, () => GUI.skin.button);

    public string Label
    {
        get => label;
        set
        {
            label = value;
            buttonContent = new(label);
        }
    }

    public Texture Icon
    {
        get => icon;
        set
        {
            icon = value;
            buttonContent = new(icon);
        }
    }

    public float Interval
    {
        get => interval;
        set
        {
            interval = value;
            scaledInterval = value * 0.01f;
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        if (GUILayout.RepeatButton(buttonContent, buttonStyle, layoutOptions))
        {
            if (!clicked)
            {
                clicked = true;
                startClickTime = Time.time;
                holdTime = Time.time;
                OnControlEvent(EventArgs.Empty);
            }
            else
            {
                if (Time.time - startClickTime >= 1f)
                {
                    if (Time.time - holdTime >= scaledInterval)
                    {
                        holdTime = Time.time;
                        OnControlEvent(EventArgs.Empty);
                    }
                }
            }
        }

        if (clicked && !UnityEngine.Input.GetMouseButton(0) && Event.current.type is EventType.Repaint)
            clicked = false;
    }
}
