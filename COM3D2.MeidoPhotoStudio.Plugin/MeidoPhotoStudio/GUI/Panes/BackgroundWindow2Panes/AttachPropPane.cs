using System;
using System.Linq;
using System.Collections.Generic;
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

            this.meidoManager.EndCallMeidos += (s, a) => SetMeidoDropdown();
            this.propManager.DoguSelectChange += (s, a) => SwitchDogu();
            this.propManager.DoguListChange += (s, a) => doguDropdownActive = this.propManager.DoguCount > 0;

            this.meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });
            this.meidoDropdown.SelectionChange += (s, a) => SwitchMaid();

            this.previousMaidButton = new Button("<");
            this.previousMaidButton.ControlEvent += (s, a) => this.meidoDropdown.Step(-1);

            this.nextMaidButton = new Button(">");
            this.nextMaidButton.ControlEvent += (s, a) => this.meidoDropdown.Step(1);

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

            keepWorldPositionToggle.Draw();

            DrawToggleGroup(AttachPoint.Head, AttachPoint.Neck);
            DrawToggleGroup(AttachPoint.UpperArmR, AttachPoint.UpperArmL);
            DrawToggleGroup(AttachPoint.ForearmR, AttachPoint.ForearmL);
            DrawToggleGroup(AttachPoint.MuneR, AttachPoint.MuneL);
            DrawToggleGroup(AttachPoint.HandR, AttachPoint.Pelvis, AttachPoint.HandL);
            DrawToggleGroup(AttachPoint.ThighR, AttachPoint.ThighL);
            DrawToggleGroup(AttachPoint.CalfR, AttachPoint.CalfL);
            DrawToggleGroup(AttachPoint.FootR, AttachPoint.FootL);

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
            foreach (Toggle toggle in Toggles.Values)
            {
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
                this.propManager.CurrentDoguIndex, point, meido, this.keepWorldPositionToggle.Value
            );
        }

        private void SwitchMaid()
        {
            if (updating || selectedMaid == this.meidoDropdown.SelectedItemIndex) return;
            selectedMaid = this.meidoDropdown.SelectedItemIndex;
            DragPointDogu dragDogu = this.propManager.CurrentDogu;
            if (dragDogu != null)
            {
                if (dragDogu.attachPointInfo.AttachPoint == AttachPoint.None) return;
                ChangeAttachPoint(dragDogu.attachPointInfo.AttachPoint);
            }
        }

        private void SwitchDogu()
        {
            if (updating) return;
            DragPointDogu dragDogu = this.propManager.CurrentDogu;
            if (dragDogu != null) SetAttachPointToggle(dragDogu.attachPointInfo.AttachPoint, true);
        }

        private void SetMeidoDropdown()
        {
            if (this.meidoManager.ActiveMeidoList.Count == 0)
            {
                SetAttachPointToggle(AttachPoint.Head, false);
            }
            int index = Mathf.Clamp(this.meidoDropdown.SelectedItemIndex, 0, this.meidoManager.ActiveMeidoList.Count);

            string[] dropdownList = this.meidoManager.ActiveMeidoList.Count == 0
                ? new[] { Translation.Get("systemMessage", "noMaids") }
                : this.meidoManager.ActiveMeidoList.Select(
                    meido => $"{meido.Slot + 1}: {meido.FirstName} {meido.LastName}"
                ).ToArray();
            this.updating = true;
            this.meidoDropdown.SetDropdownItems(dropdownList, index);
            this.updating = false;

            meidoDropdownActive = this.meidoManager.HasActiveMeido;
        }
    }
}
