using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropManagerPane : BasePane
    {
        private PropManager propManager;
        private Dropdown propDropdown;
        private Button previousPropButton;
        private Button nextPropButton;
        private Toggle dragPointToggle;
        private Toggle gizmoToggle;
        private Button deletePropButton;
        private Button copyPropButton;
        private int CurrentDoguIndex => this.propManager.CurrentDoguIndex;

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
                this.updating = true;
                this.propDropdown.SelectedItemIndex = CurrentDoguIndex;
                this.updating = false;
                UpdateToggles();
            };

            this.propDropdown = new Dropdown(this.propManager.PropNameList);
            this.propDropdown.SelectionChange += (s, a) =>
            {
                if (updating) return;
                this.propManager.SetCurrentDogu(this.propDropdown.SelectedItemIndex);
                UpdateToggles();
            };

            this.previousPropButton = new Button("<");
            this.previousPropButton.ControlEvent += (s, a) => this.propDropdown.Step(-1);

            this.nextPropButton = new Button(">");
            this.nextPropButton.ControlEvent += (s, a) => this.propDropdown.Step(1);

            this.dragPointToggle = new Toggle("Cube");
            this.dragPointToggle.ControlEvent += (s, a) =>
            {
                if (this.updating || this.propManager.DoguCount == 0) return;
                this.propManager.CurrentDogu.DragPointEnabled = dragPointToggle.Value;
            };

            this.gizmoToggle = new Toggle("Gizmo");
            this.gizmoToggle.ControlEvent += (s, a) =>
            {
                if (this.updating || this.propManager.DoguCount == 0) return;
                this.propManager.CurrentDogu.GizmoEnabled = gizmoToggle.Value;
            };

            this.copyPropButton = new Button("Copy");
            this.copyPropButton.ControlEvent += (s, a) => this.propManager.CopyDogu(CurrentDoguIndex);

            this.deletePropButton = new Button("Delete");
            this.deletePropButton.ControlEvent += (s, a) => this.propManager.RemoveDogu(CurrentDoguIndex);
        }

        public override void Draw()
        {
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 140f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MiscGUI.WhiteLine();

            GUI.enabled = this.propManager.DoguCount > 0;

            GUILayout.BeginHorizontal();
            this.propDropdown.Draw(dropdownLayoutOptions);
            this.previousPropButton.Draw(arrowLayoutOptions);
            this.nextPropButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUILayoutOption noExpandWidth = GUILayout.ExpandWidth(false);

            GUILayout.BeginHorizontal();
            this.dragPointToggle.Draw(noExpandWidth);
            this.gizmoToggle.Draw(noExpandWidth);
            this.copyPropButton.Draw(noExpandWidth);
            this.deletePropButton.Draw(noExpandWidth);
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void UpdatePropList()
        {
            this.updating = true;
            this.propDropdown.SetDropdownItems(this.propManager.PropNameList, CurrentDoguIndex);
            this.updating = false;
        }

        private void UpdateToggles()
        {
            DragPointDogu dogu = this.propManager.CurrentDogu;
            if (dogu == null) return;

            this.updating = true;
            this.dragPointToggle.Value = dogu.DragPointEnabled;
            this.gizmoToggle.Value = dogu.GizmoEnabled;
            this.updating = false;
        }
    }
}
