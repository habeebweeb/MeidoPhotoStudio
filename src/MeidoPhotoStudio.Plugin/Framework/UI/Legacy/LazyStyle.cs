namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class LazyStyle
{
    private readonly Func<GUIStyle> styleProvider;

    private int fontSize;
    private GUIStyle style;

    public LazyStyle(int fontSize, Func<GUIStyle> styleProvider)
    {
        this.styleProvider = styleProvider ?? throw new ArgumentNullException(nameof(styleProvider));
        this.fontSize = fontSize;

        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;
    }

    public GUIStyle Style
    {
        get
        {
            if (style is not null)
                return style;

            style = styleProvider();

            style.fontSize = Utility.GetPix(fontSize);

            return style;
        }
    }

    public int FontSize
    {
        get => fontSize;
        set
        {
            if (fontSize == value)
                return;

            fontSize = value;

            if (style is null)
                return;

            style.fontSize = Utility.GetPix(fontSize);
        }
    }

    public static implicit operator GUIStyle(LazyStyle style) =>
        style.Style;

    public bool TrySet(Action<GUIStyle> setter)
    {
        try
        {
            setter(Style);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private void OnScreenSizeChanged(object sender, EventArgs e)
    {
        if (style is null)
            return;

        style.fontSize = Utility.GetPix(fontSize);
    }
}
