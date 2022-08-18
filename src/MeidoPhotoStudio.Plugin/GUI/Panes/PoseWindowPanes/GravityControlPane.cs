using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class GravityControlPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Toggle hairToggle;
    private readonly Toggle skirtToggle;
    private readonly Toggle globalToggle;

    private string header;

    public GravityControlPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        hairToggle = new(Translation.Get("gravityControlPane", "hairToggle"));
        hairToggle.ControlEvent += (_, _) =>
            ToggleGravity(hairToggle.Value, skirt: false);

        skirtToggle = new(Translation.Get("gravityControlPane", "skirtToggle"));
        skirtToggle.ControlEvent += (_, _) =>
            ToggleGravity(skirtToggle.Value, skirt: true);

        globalToggle = new(Translation.Get("gravityControlPane", "globalToggle"));
        globalToggle.ControlEvent += (_, _) =>
            SetGlobalGravity(globalToggle.Value);

        header = Translation.Get("gravityControlPane", "gravityHeader");
    }

    public override void Draw()
    {
        var enabled = meidoManager.HasActiveMeido;

        GUI.enabled = enabled;

        MpsGui.Header(header);
        MpsGui.WhiteLine();

        var meido = meidoManager.ActiveMeido;

        GUILayout.BeginHorizontal();

        GUI.enabled = enabled && meido.HairGravityControl.Valid;
        hairToggle.Draw();

        GUI.enabled = enabled && meido.SkirtGravityControl.Valid;
        skirtToggle.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;
        globalToggle.Draw();

        GUI.enabled = true;
    }

    public override void UpdatePane()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        var meido = meidoManager.ActiveMeido;

        updating = true;

        hairToggle.Value = meido.HairGravityActive;
        skirtToggle.Value = meido.SkirtGravityActive;

        updating = false;
    }

    protected override void ReloadTranslation()
    {
        hairToggle.Label = Translation.Get("gravityControlPane", "hairToggle");
        skirtToggle.Label = Translation.Get("gravityControlPane", "skirtToggle");
        globalToggle.Label = Translation.Get("gravityControlPane", "globalToggle");
        header = Translation.Get("gravityControlPane", "gravityHeader");
    }

    private void ToggleGravity(bool value, bool skirt = false)
    {
        if (updating)
            return;

        if (meidoManager.GlobalGravity)
        {
            foreach (var meido in meidoManager.ActiveMeidoList)
            {
                if (skirt)
                    meido.SkirtGravityActive = value;
                else
                    meido.HairGravityActive = value;
            }
        }
        else
        {
            if (skirt)
                meidoManager.ActiveMeido.SkirtGravityActive = value;
            else
                meidoManager.ActiveMeido.HairGravityActive = value;
        }
    }

    private void SetGlobalGravity(bool value) =>
        meidoManager.GlobalGravity = value;
}
