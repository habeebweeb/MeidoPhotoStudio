using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidFreeLookPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Slider lookXSlider;
    private readonly Slider lookYSlider;
    private readonly Toggle headToCamToggle;
    private readonly Toggle eyeToCamToggle;

    private string bindLabel;

    public MaidFreeLookPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        lookXSlider = new(Translation.Get("freeLookPane", "xSlider"), -0.6f, 0.6f);
        lookXSlider.ControlEvent += (_, _) =>
            SetMaidLook();

        lookYSlider = new(Translation.Get("freeLookPane", "ySlider"), 0.5f, -0.55f);
        lookYSlider.ControlEvent += (_, _) =>
            SetMaidLook();

        headToCamToggle = new(Translation.Get("freeLookPane", "headToCamToggle"));
        headToCamToggle.ControlEvent += (_, _) =>
            SetHeadToCam(headToCamToggle.Value, eye: false);

        eyeToCamToggle = new(Translation.Get("freeLookPane", "eyeToCamToggle"));
        eyeToCamToggle.ControlEvent += (_, _) =>
            SetHeadToCam(eyeToCamToggle.Value, eye: true);

        bindLabel = Translation.Get("freeLookPane", "bindLabel");
    }

    public override void Draw()
    {
        GUI.enabled = meidoManager.HasActiveMeido && meidoManager.ActiveMeido.FreeLook;
        GUILayout.BeginHorizontal();
        lookXSlider.Draw();
        lookYSlider.Draw();
        GUILayout.EndHorizontal();

        GUI.enabled = meidoManager.HasActiveMeido;

        GUILayout.BeginHorizontal();
        GUILayout.Label(bindLabel, GUILayout.ExpandWidth(false));
        eyeToCamToggle.Draw();
        headToCamToggle.Draw();
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    public override void UpdatePane()
    {
        var meido = meidoManager.ActiveMeido;

        updating = true;
        SetBounds();
        lookXSlider.Value = meido.Body.offsetLookTarget.z;
        lookYSlider.Value = meido.Body.offsetLookTarget.x;
        eyeToCamToggle.Value = meido.EyeToCam;
        headToCamToggle.Value = meido.HeadToCam;
        updating = false;
    }

    public void SetHeadToCam(bool value, bool eye = false)
    {
        if (updating)
            return;

        var meido = meidoManager.ActiveMeido;

        if (eye)
            meido.EyeToCam = value;
        else
            meido.HeadToCam = value;
    }

    public void SetMaidLook()
    {
        if (updating)
            return;

        var body = meidoManager.ActiveMeido.Body;

        body.offsetLookTarget = new(lookYSlider.Value, 1f, lookXSlider.Value);
    }

    public void SetBounds()
    {
        var left = 0.5f;
        var right = -0.55f;

        if (meidoManager.ActiveMeido.Stop)
        {
            left *= 0.6f;
            right *= 0.6f;
        }

        lookYSlider.SetBounds(left, right);
    }

    protected override void ReloadTranslation()
    {
        lookXSlider.Label = Translation.Get("freeLookPane", "xSlider");
        lookYSlider.Label = Translation.Get("freeLookPane", "ySlider");
        headToCamToggle.Label = Translation.Get("freeLookPane", "headToCamToggle");
        eyeToCamToggle.Label = Translation.Get("freeLookPane", "eyeToCamToggle");
        bindLabel = Translation.Get("freeLookPane", "bindLabel");
    }
}
