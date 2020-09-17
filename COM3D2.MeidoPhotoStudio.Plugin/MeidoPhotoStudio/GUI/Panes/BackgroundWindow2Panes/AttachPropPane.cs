using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class AttachPropPane : BasePane
    {
        private readonly PropManager propManager;
        private readonly MeidoManager meidoManager;
        private readonly Dictionary<AttachPoint, Toggle> Toggles = new Dictionary<AttachPoint, Toggle>();
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
        private readonly Toggle keepWorldPositionToggle;
        private readonly Button previousMaidButton;
        private readonly Button nextMaidButton;
        private readonly Dropdown meidoDropdown;
        private bool meidoDropdownActive;
        private bool doguDropdownActive;
        private bool PaneActive => meidoDropdownActive && doguDropdownActive;
        private string header;
        private int selectedMaid;

        public AttachPropPane(MeidoManager meidoManager, PropManager propManager)
        {
            header = Translation.Get("attachPropPane", "header");
            this.propManager = propManager;
            this.meidoManager = meidoManager;

            this.meidoManager.EndCallMeidos += (s, a) => SetMeidoDropdown();
            this.propManager.DoguSelectChange += (s, a) => SwitchDogu();
            this.propManager.DoguListChange += (s, a) => doguDropdownActive = this.propManager.DoguCount > 0;

            meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });
            meidoDropdown.SelectionChange += (s, a) => SwitchMaid();

            previousMaidButton = new Button("<");
            previousMaidButton.ControlEvent += (s, a) => meidoDropdown.Step(-1);

            nextMaidButton = new Button(">");
            nextMaidButton.ControlEvent += (s, a) => meidoDropdown.Step(1);

            keepWorldPositionToggle = new Toggle(Translation.Get("attachPropPane", "keepWorldPosition"));

            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;
                AttachPoint point = attachPoint;
                Toggle toggle = new Toggle(Translation.Get("attachPropPane", toggleTranslation[point]));
                toggle.ControlEvent += (s, a) =>
                {
                    if (updating) return;
                    ChangeAttachPoint(point);
                };
                Toggles[point] = toggle;
            }
        }

        protected override void ReloadTranslation()
        {
            header = Translation.Get("attachPropPane", "header");
            keepWorldPositionToggle.Label = Translation.Get("attachPropPane", "keepWorldPosition");
            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;
                Toggles[attachPoint].Label = Translation.Get("attachPropPane", toggleTranslation[attachPoint]);
            }
        }

        public override void Draw()
        {
            const float dropdownButtonHeight = 30;
            const float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            MpsGui.Header(header);
            MpsGui.WhiteLine();

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
            updating = true;
            foreach (Toggle toggle in Toggles.Values)
            {
                toggle.Value = false;
            }
            if (point != AttachPoint.None) Toggles[point].Value = value;
            updating = false;
        }

        private void ChangeAttachPoint(AttachPoint point)
        {
            bool toggleValue = point != AttachPoint.None && Toggles[point].Value;
            SetAttachPointToggle(point, toggleValue);

            Meido meido = null;

            if (point != AttachPoint.None)
            {
                meido = Toggles[point].Value
                    ? meidoManager.ActiveMeidoList[meidoDropdown.SelectedItemIndex]
                    : null;
            }

            propManager.AttachProp(
                propManager.CurrentDoguIndex, point, meido, keepWorldPositionToggle.Value
            );
        }

        private void SwitchMaid()
        {
            if (updating || selectedMaid == meidoDropdown.SelectedItemIndex) return;
            selectedMaid = meidoDropdown.SelectedItemIndex;
            DragPointDogu dragDogu = propManager.CurrentDogu;
            if (dragDogu != null)
            {
                if (dragDogu.attachPointInfo.AttachPoint == AttachPoint.None) return;
                ChangeAttachPoint(dragDogu.attachPointInfo.AttachPoint);
            }
        }

        private void SwitchDogu()
        {
            if (updating) return;
            DragPointDogu dragDogu = propManager.CurrentDogu;
            if (dragDogu != null) SetAttachPointToggle(dragDogu.attachPointInfo.AttachPoint, true);
        }

        private void SetMeidoDropdown()
        {
            if (meidoManager.ActiveMeidoList.Count == 0)
            {
                SetAttachPointToggle(AttachPoint.Head, false);
            }
            int index = Mathf.Clamp(meidoDropdown.SelectedItemIndex, 0, meidoManager.ActiveMeidoList.Count);

            string[] dropdownList = meidoManager.ActiveMeidoList.Count == 0
                ? new[] { Translation.Get("systemMessage", "noMaids") }
                : meidoManager.ActiveMeidoList.Select(
                    meido => $"{meido.Slot + 1}: {meido.FirstName} {meido.LastName}"
                ).ToArray();
            updating = true;
            meidoDropdown.SetDropdownItems(dropdownList, index);
            updating = false;

            meidoDropdownActive = meidoManager.HasActiveMeido;
        }
    }
}
