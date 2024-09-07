using MeidoPhotoStudio.Plugin.Core.Effects;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SepiaTonePane(SepiaToneController effectController) : EffectPane<SepiaToneController>(effectController)
{
    public override void Draw() =>
        effectActiveToggle.Draw();
}
