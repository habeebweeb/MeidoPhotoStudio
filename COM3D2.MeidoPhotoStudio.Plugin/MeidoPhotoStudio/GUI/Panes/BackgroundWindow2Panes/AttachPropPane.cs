using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class AttachPropPane : BasePane
    {
        private PropManager propManager;
        private MeidoManager meidoManager;
        private Dictionary<AttachPoint, Toggle> Toggles = new Dictionary<AttachPoint, Toggle>();
        private static readonly Dictionary<AttachPoint, string> toggleTranslation =
            new Dictionary<AttachPoint, string>()
            {
                [AttachPoint.Head] = "head",
                [AttachPoint.Neck] = "neck",
                [AttachPoint.UpperArmL] = "upperArmL",
                [AttachPoint.UpperArmR] = "upperArmR",
                [AttachPoint.ForearmL] = "forearmL",
                [AttachPoint.ForearmR] = "forearmR",
                [AttachPoint.MuneL] = "muneL",
                [AttachPoint.MuneR] = "muneR",
                [AttachPoint.HandL] = "handL",
                [AttachPoint.HandR] = "handR",
                [AttachPoint.Pelvis] = "pelvis",
                [AttachPoint.ThighL] = "thighL",
                [AttachPoint.ThighR] = "thighR",
                [AttachPoint.CalfL] = "calfL",
                [AttachPoint.CalfR] = "calfR",
                [AttachPoint.FootL] = "footL",
                [AttachPoint.FootR] = "footR",
            };
        private Toggle keepWorldPositionToggle;
        private Button previousMaidButton;
        private Button nextMaidButton;
        private Dropdown meidoDropdown;
        private Button previousDoguButton;
        private Button nextDoguButton;
        private Dropdown doguDropdown;
        private bool meidoDropdownActive = false;
        private bool doguDropdownActive = false;
        private bool PaneActive => meidoDropdownActive && doguDropdownActive;
        private string header;
        private int selectedMaid = 0;

        public AttachPropPane(MeidoManager meidoManager, PropManager propManager)
        {
            this.header = Translation.Get("attachPropPane", "header");
            this.propManager = propManager;
            this.meidoManager = meidoManager;

            this.propManager.DoguListChange += (s, a) => SetDoguDropdown();
            this.meidoManager.EndCallMeidos += (s, a) => SetMeidoDropdown();

            this.meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });
            this.meidoDropdown.SelectionChange += (s, a) => SwitchMaid();

            this.previousMaidButton = new Button("<");
            this.previousMaidButton.ControlEvent += (s, a) => this.meidoDropdown.Step(-1);

            this.nextMaidButton = new Button(">");
            this.nextMaidButton.ControlEvent += (s, a) => this.meidoDropdown.Step(1);

            this.doguDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noProps") });
            this.doguDropdown.SelectionChange += (s, a) => SwitchDogu();

            this.previousDoguButton = new Button("<");
            this.previousDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(-1);

            this.nextDoguButton = new Button(">");
            this.nextDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(1);

            this.keepWorldPositionToggle = new Toggle(Translation.Get("attachPropPane", "keepWorldPosition"));

            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;
                AttachPoint point = attachPoint;
                Toggle toggle = new Toggle(Translation.Get("attachPropPane", toggleTranslation[point]));
                toggle.ControlEvent += (s, a) =>
                {
                    if (this.updating) return;
                    ChangeAttachPoint(point);
                };
                Toggles[point] = toggle;
            }
        }

        protected override void ReloadTranslation()
        {
            this.header = Translation.Get("attachPropPane", "header");
            this.keepWorldPositionToggle.Label = Translation.Get("attachPropPane", "keepWorldPosition");
            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;
                Toggles[attachPoint].Label = Translation.Get("attachPropPane", toggleTranslation[attachPoint]);
            }
        }

        public override void Draw()
        {
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MiscGUI.Header(this.header);
            MiscGUI.WhiteLine();

            GUI.enabled = PaneActive;

            meidoDropdown.Draw(dropdownLayoutOptions);

            GUILayout.BeginHorizontal();
            doguDropdown.Draw(dropdownLayoutOptions);
            previousDoguButton.Draw(arrowLayoutOptions);
            nextDoguButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            keepWorldPositionToggle.Draw();

            DrawToggleGroup(AttachPoint.Head, AttachPoint.Neck);
            DrawToggleGroup(AttachPoint.UpperArmL, AttachPoint.UpperArmR);
            DrawToggleGroup(AttachPoint.ForearmL, AttachPoint.ForearmR);
            DrawToggleGroup(AttachPoint.MuneL, AttachPoint.MuneR);
            DrawToggleGroup(AttachPoint.HandL, AttachPoint.Pelvis, AttachPoint.HandR);
            DrawToggleGroup(AttachPoint.ThighL, AttachPoint.ThighR);
            DrawToggleGroup(AttachPoint.CalfL, AttachPoint.CalfR);
            DrawToggleGroup(AttachPoint.FootL, AttachPoint.FootR);

            GUI.enabled = true;
        }

        private void DrawToggleGroup(params AttachPoint[] attachPoints)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (AttachPoint point in attachPoints)
            {
                Toggles[point].Draw();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void SetAttachPointToggle(AttachPoint point, bool value)
        {
            this.updating = true;
            foreach (KeyValuePair<AttachPoint, Toggle> kvp in Toggles)
            {
                Toggle toggle = kvp.Value;
                toggle.Value = false;
            }
            if (point != AttachPoint.None) Toggles[point].Value = value;
            this.updating = false;
        }

        private void ChangeAttachPoint(AttachPoint point)
        {
            bool toggleValue = point == AttachPoint.None ? false : Toggles[point].Value;
            SetAttachPointToggle(point, toggleValue);

            Meido meido = null;

            if (point != AttachPoint.None)
            {
                meido = Toggles[point].Value
                    ? this.meidoManager.ActiveMeidoList[this.meidoDropdown.SelectedItemIndex]
                    : null;
            }

            this.propManager.AttachProp(
                this.doguDropdown.SelectedItemIndex, point, meido, this.keepWorldPositionToggle.Value
            );
        }

        private void SwitchMaid()
        {
            if (updating || selectedMaid == this.meidoDropdown.SelectedItemIndex) return;
            selectedMaid = this.meidoDropdown.SelectedItemIndex;
            DragPointDogu dragDogu = this.propManager.GetDogu(this.doguDropdown.SelectedItemIndex);
            if (dragDogu != null)
            {
                if (dragDogu.attachPointInfo.AttachPoint == AttachPoint.None) return;
                ChangeAttachPoint(dragDogu.attachPointInfo.AttachPoint);
            }
        }

        private void SwitchDogu()
        {
            if (updating) return;
            DragPointDogu dragDogu = this.propManager.GetDogu(this.doguDropdown.SelectedItemIndex);
            if (dragDogu != null) SetAttachPointToggle(dragDogu.attachPointInfo.AttachPoint, true);
        }

        private void SetDoguDropdown()
        {
            if (this.propManager.DoguCount == 0)
            {
                SetAttachPointToggle(AttachPoint.Head, false);
            }
            int index = Mathf.Clamp(this.doguDropdown.SelectedItemIndex, 0, this.propManager.DoguCount);

            this.doguDropdown.SetDropdownItems(this.propManager.PropNameList, index);

            doguDropdownActive = this.propManager.DoguCount != 0;
        }

        private void SetMeidoDropdown()
        {
            if (this.meidoManager.ActiveMeidoList.Count == 0)
            {
                SetAttachPointToggle(AttachPoint.Head, false);
            }
            int index = Mathf.Clamp(this.meidoDropdown.SelectedItemIndex, 0, this.meidoManager.ActiveMeidoList.Count);

            this.updating = true;
            this.meidoDropdown.SetDropdownItems(this.meidoManager.ActiveMeidoNameList, index);
            this.updating = false;

            meidoDropdownActive = this.meidoManager.HasActiveMeido;
        }
    }
}
