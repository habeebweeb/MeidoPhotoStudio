using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidIKPane : BasePane
    {
        private MeidoManager meidoManager;
        private Toggle ikToggle;
        private Toggle releaseIKToggle;
        private Toggle boneIKToggle;
        private enum IKToggle
        {
            IK, Release, Bone
        }

        public MaidIKPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            this.ikToggle = new Toggle(Translation.Get("maidPoseWindow", "ikToggle"), true);
            this.ikToggle.ControlEvent += (s, a) => SetIK(IKToggle.IK, this.ikToggle.Value);

            this.releaseIKToggle = new Toggle(Translation.Get("maidPoseWindow", "releaseToggle"));
            this.releaseIKToggle.ControlEvent += (s, a) => SetIK(IKToggle.Release, this.releaseIKToggle.Value);

            this.boneIKToggle = new Toggle(Translation.Get("maidPoseWindow", "boneToggle"));
            this.boneIKToggle.ControlEvent += (s, a) => SetIK(IKToggle.Bone, this.boneIKToggle.Value);
        }

        protected override void ReloadTranslation()
        {
            this.ikToggle.Label = Translation.Get("maidPoseWindow", "ikToggle");
            this.releaseIKToggle.Label = Translation.Get("maidPoseWindow", "releaseToggle");
            this.boneIKToggle.Label = Translation.Get("maidPoseWindow", "boneToggle");
        }

        private void SetIK(IKToggle toggle, bool value)
        {
            if (updating) return;
            if (toggle == IKToggle.IK) this.meidoManager.ActiveMeido.IsIK = value;
            else if (toggle == IKToggle.Release) this.meidoManager.ActiveMeido.IsStop = false;
            else if (toggle == IKToggle.Bone) this.meidoManager.ActiveMeido.IsBone = value;
        }

        public override void UpdatePane()
        {
            this.updating = true;
            this.ikToggle.Value = this.meidoManager.ActiveMeido.IsIK;
            this.releaseIKToggle.Value = this.meidoManager.ActiveMeido.IsStop;
            this.boneIKToggle.Value = this.meidoManager.ActiveMeido.IsBone;
            this.updating = false;
        }

        public override void Draw()
        {
            bool active = this.meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            GUI.enabled = active;
            this.ikToggle.Draw();

            GUI.enabled = active ? this.meidoManager.ActiveMeido.IsStop : false;
            this.releaseIKToggle.Draw();

            GUI.enabled = active ? this.ikToggle.Value : false;
            this.boneIKToggle.Draw();
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}
