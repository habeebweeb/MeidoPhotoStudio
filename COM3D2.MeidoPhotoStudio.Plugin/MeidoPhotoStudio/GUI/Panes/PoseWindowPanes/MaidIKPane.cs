using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidIKPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Toggle ikToggle;
        private readonly Toggle releaseIKToggle;
        private readonly Toggle boneIKToggle;
        private enum IKToggle
        {
            IK, Release, Bone
        }

        public MaidIKPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            ikToggle = new Toggle(Translation.Get("maidPoseWindow", "ikToggle"), true);
            ikToggle.ControlEvent += (s, a) => SetIK(IKToggle.IK, ikToggle.Value);

            releaseIKToggle = new Toggle(Translation.Get("maidPoseWindow", "releaseToggle"));
            releaseIKToggle.ControlEvent += (s, a) => SetIK(IKToggle.Release, releaseIKToggle.Value);

            boneIKToggle = new Toggle(Translation.Get("maidPoseWindow", "boneToggle"));
            boneIKToggle.ControlEvent += (s, a) => SetIK(IKToggle.Bone, boneIKToggle.Value);
        }

        protected override void ReloadTranslation()
        {
            ikToggle.Label = Translation.Get("maidPoseWindow", "ikToggle");
            releaseIKToggle.Label = Translation.Get("maidPoseWindow", "releaseToggle");
            boneIKToggle.Label = Translation.Get("maidPoseWindow", "boneToggle");
        }

        private void SetIK(IKToggle toggle, bool value)
        {
            if (updating) return;
            if (toggle == IKToggle.IK) meidoManager.ActiveMeido.IK = value;
            else if (toggle == IKToggle.Release) meidoManager.ActiveMeido.Stop = false;
            else if (toggle == IKToggle.Bone) meidoManager.ActiveMeido.Bone = value;
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
            bool active = meidoManager.HasActiveMeido;

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
    }
}
