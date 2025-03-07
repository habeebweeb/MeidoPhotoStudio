using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Api;

public class UIApi(PluginCore pluginCore, WindowManager windowManager)
    : ApiBase(pluginCore)
{
    public bool Visible
    {
        get
        {
            Valid();

            return windowManager.Visible;
        }

        set
        {
            Valid();

            windowManager.Visible = value;
        }
    }

    public bool MainWindowVisible
    {
        get
        {
            Valid();

            return windowManager[WindowManager.Window.Main].Visible;
        }

        set
        {
            Valid();

            windowManager[WindowManager.Window.Main].Visible = value;
        }
    }
}
