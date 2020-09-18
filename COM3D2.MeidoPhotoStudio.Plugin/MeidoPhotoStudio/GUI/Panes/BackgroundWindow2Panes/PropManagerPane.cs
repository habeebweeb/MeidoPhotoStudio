using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropManagerPane : BasePane
    {
        private readonly PropManager propManager;
        private readonly Dropdown propDropdown;
        private readonly Button previousPropButton;
        private readonly Button nextPropButton;
        private readonly Toggle dragPointToggle;
        private readonly Toggle gizmoToggle;
        private readonly Toggle shadowCastingToggle;
        private readonly Button deletePropButton;
        private readonly Button copyPropButton;
        private string propManagerHeader;

        private int CurrentDoguIndex => propManager.CurrentDoguIndex;

        public PropManagerPane(PropManager propManager)
        {
            this.propManager = propManager;
            this.propManager.DoguListChange += (s, a) =>
            {
                UpdatePropList();
                UpdateToggles();
            };

            this.propManager.DoguSelectChange += (s, a) =>
            {
                updating = true;
                propDropdown.SelectedItemIndex = CurrentDoguIndex;
                updating = false;
                UpdateToggles();
            };

            propDropdown = new Dropdown(this.propManager.PropNameList);
            propDropdown.SelectionChange += (s, a) =>
            {
                if (updating) return;
                this.propManager.SetCurrentDogu(propDropdown.SelectedItemIndex);
                UpdateToggles();
            };

            previousPropButton = new Button("<");
            previousPropButton.ControlEvent += (s, a) => propDropdown.Step(-1);

            nextPropButton = new Button(">");
            nextPropButton.ControlEvent += (s, a) => propDropdown.Step(1);

            dragPointToggle = new Toggle(Translation.Get("propManagerPane", "dragPointToggle"));
            dragPointToggle.ControlEvent += (s, a) =>
            {
                if (updating || this.propManager.DoguCount == 0) return;
                this.propManager.CurrentDogu.DragPointEnabled = dragPointToggle.Value;
            };

            gizmoToggle = new Toggle(Translation.Get("propManagerPane", "gizmoToggle"));
            gizmoToggle.ControlEvent += (s, a) =>
            {
                if (updating || this.propManager.DoguCount == 0) return;
                this.propManager.CurrentDogu.GizmoEnabled = gizmoToggle.Value;
            };

            shadowCastingToggle = new Toggle(Translation.Get("propManagerPane", "shadowCastingToggle"));
            shadowCastingToggle.ControlEvent += (s, a) =>
            {
                if (updating || this.propManager.DoguCount == 0) return;
                this.propManager.CurrentDogu.ShadowCasting = shadowCastingToggle.Value;
            };

            copyPropButton = new Button(Translation.Get("propManagerPane", "copyButton"));
            copyPropButton.ControlEvent += (s, a) => this.propManager.CopyDogu(CurrentDoguIndex);

            deletePropButton = new Button(Translation.Get("propManagerPane", "deleteButton"));
            deletePropButton.ControlEvent += (s, a) => this.propManager.RemoveDogu(CurrentDoguIndex);

            propManagerHeader = Translation.Get("propManagerPane", "header");
        }

        protected override void ReloadTranslation()
        {
            dragPointToggle.Label = Translation.Get("propManagerPane", "dragPointToggle");
            gizmoToggle.Label = Translation.Get("propManagerPane", "gizmoToggle");
            shadowCastingToggle.Label = Translation.Get("propManagerPane", "shadowCastingToggle");
            copyPropButton.Label = Translation.Get("propManagerPane", "copyButton");
            deletePropButton.Label = Translation.Get("propManagerPane", "deleteButton");
            propManagerHeader = Translation.Get("propManagerPane", "header");
        }

        public override void Draw()
        {
            const float buttonHeight = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(buttonHeight),
                GUILayout.Height(buttonHeight)
            };

            const float dropdownButtonWidth = 140f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(buttonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MpsGui.Header(propManagerHeader);
            MpsGui.WhiteLine();

            GUI.enabled = propManager.DoguCount > 0;

            GUILayout.BeginHorizontal();
            propDropdown.Draw(dropdownLayoutOptions);
            previousPropButton.Draw(arrowLayoutOptions);
            nextPropButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayoutOption noExpandWidth = GUILayout.ExpandWidth(false);

            GUILayout.BeginHorizontal();
            dragPointToggle.Draw(noExpandWidth);
            gizmoToggle.Draw(noExpandWidth);
            copyPropButton.Draw(noExpandWidth);
            deletePropButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            shadowCastingToggle.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void UpdatePropList()
        {
            updating = true;
            propDropdown.SetDropdownItems(propManager.PropNameList, CurrentDoguIndex);
            updating = false;
        }

        private void UpdateToggles()
        {
            DragPointDogu dogu = propManager.CurrentDogu;
            if (dogu == null) return;

            updating = true;
            dragPointToggle.Value = dogu.DragPointEnabled;
            gizmoToggle.Value = dogu.GizmoEnabled;
            shadowCastingToggle.Value = dogu.ShadowCasting;
            updating = false;
        }
    }
}
