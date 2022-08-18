using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidIKPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Toggle ikToggle;
    private readonly Toggle releaseIKToggle;
    private readonly Toggle boneIKToggle;

    public MaidIKPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        ikToggle = new(Translation.Get("maidPoseWindow", "ikToggle"), true);
        ikToggle.ControlEvent += (_, _) =>
            SetIK(IKToggle.IK, ikToggle.Value);

        releaseIKToggle = new(Translation.Get("maidPoseWindow", "releaseToggle"));
        releaseIKToggle.ControlEvent += (_, _) =>
            SetIK(IKToggle.Release, releaseIKToggle.Value);

        boneIKToggle = new(Translation.Get("maidPoseWindow", "boneToggle"));
        boneIKToggle.ControlEvent += (_, _) =>
            SetIK(IKToggle.Bone, boneIKToggle.Value);
    }

    private enum IKToggle
    {
        IK,
        Release,
        Bone,
    }

    public override void UpdatePane()
    {
        updating = true;
        ikToggle.Value = meidoManager.ActiveMeido.IK;
        releaseIKToggle.Value = meidoManager.ActiveMeido.Stop;
        boneIKToggle.Value = meidoManager.ActiveMeido.Bone;
        updating = false;
    }

    public override void Draw()
    {
        var active = meidoManager.HasActiveMeido;

        GUILayout.BeginHorizontal();
        GUI.enabled = active;
        ikToggle.Draw();

        GUI.enabled = active && meidoManager.ActiveMeido.Stop;
        releaseIKToggle.Draw();

        GUI.enabled = active && ikToggle.Value;
        boneIKToggle.Draw();
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        ikToggle.Label = Translation.Get("maidPoseWindow", "ikToggle");
        releaseIKToggle.Label = Translation.Get("maidPoseWindow", "releaseToggle");
        boneIKToggle.Label = Translation.Get("maidPoseWindow", "boneToggle");
    }

    private void SetIK(IKToggle toggle, bool value)
    {
        if (updating)
            return;

        if (toggle is IKToggle.IK)
            meidoManager.ActiveMeido.IK = value;
        else if (toggle is IKToggle.Release)
            meidoManager.ActiveMeido.Stop = false;
        else if (toggle is IKToggle.Bone)
            meidoManager.ActiveMeido.Bone = value;
    }
}
