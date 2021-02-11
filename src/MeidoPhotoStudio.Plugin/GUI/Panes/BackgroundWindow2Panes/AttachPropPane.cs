using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class AttachPropPane : BasePane
    {
        private readonly PropManager propManager;
        private readonly MeidoManager meidoManager;
        private readonly Dictionary<AttachPoint, Toggle> toggles = new Dictionary<AttachPoint, Toggle>();

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
                [AttachPoint.Spine1a] = "spine1a",
                [AttachPoint.Spine1] = "spine1",
                [AttachPoint.Spine0a] = "spine0a",
                [AttachPoint.Spine0] = "spine0"
            };

        private readonly Toggle keepWorldPositionToggle;
        private readonly Dropdown meidoDropdown;
        private Toggle activeToggle;
        private bool meidoDropdownActive;
        private bool doguDropdownActive;
        private string header;
        private bool PaneActive => meidoDropdownActive && doguDropdownActive;
        private Meido SelectedMeido => meidoManager.ActiveMeidoList[meidoDropdown.SelectedItemIndex];
        private DragPointProp SelectedProp => propManager.CurrentProp;
        private bool KeepWoldPosition => keepWorldPositionToggle.Value;

        public AttachPropPane(MeidoManager meidoManager, PropManager propManager)
        {
            header = Translation.Get("attachPropPane", "header");
            this.propManager = propManager;
            this.meidoManager = meidoManager;

            this.meidoManager.EndCallMeidos += (s, a) => SetMeidoDropdown();
            this.propManager.PropSelectionChange += (s, a) => UpdateToggles();

            this.propManager.PropListChange += (s, a) =>
            {
                doguDropdownActive = this.propManager.PropCount > 0;
                UpdateToggles();
            };

            meidoDropdown = new Dropdown(new[] { Translation.Get("systemMessage", "noMaids") });
            meidoDropdown.SelectionChange += (s, a) => UpdateToggles();

            keepWorldPositionToggle = new Toggle(Translation.Get("attachPropPane", "keepWorldPosition"));

            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;

                var point = attachPoint;
                var toggle = new Toggle(Translation.Get("attachPropPane", toggleTranslation[point]));
                toggle.ControlEvent += (s, a) => OnToggleChange(point);
                toggles[point] = toggle;
            }
        }

        protected override void ReloadTranslation()
        {
            header = Translation.Get("attachPropPane", "header");
            keepWorldPositionToggle.Label = Translation.Get("attachPropPane", "keepWorldPosition");
            foreach (AttachPoint attachPoint in Enum.GetValues(typeof(AttachPoint)))
            {
                if (attachPoint == AttachPoint.None) continue;

                toggles[attachPoint].Label = Translation.Get("attachPropPane", toggleTranslation[attachPoint]);
            }
        }

        public override void Draw()
        {
            const float dropdownButtonHeight = 30;
            const float dropdownButtonWidth = 153f;
            GUILayoutOption[] dropdownLayoutOptions =
            {
                GUILayout.Height(dropdownButtonHeight), GUILayout.Width(dropdownButtonWidth)
            };

            MpsGui.Header(header);
            MpsGui.WhiteLine();

            GUI.enabled = PaneActive;

            meidoDropdown.Draw(dropdownLayoutOptions);

            keepWorldPositionToggle.Draw();

            DrawToggleGroup(AttachPoint.Head, AttachPoint.Neck);
            DrawToggleGroup(AttachPoint.UpperArmR, AttachPoint.Spine1a, AttachPoint.UpperArmL);
            DrawToggleGroup(AttachPoint.ForearmR, AttachPoint.Spine1, AttachPoint.ForearmL);
            DrawToggleGroup(AttachPoint.MuneR, AttachPoint.Spine0a, AttachPoint.MuneL);
            DrawToggleGroup(AttachPoint.HandR, AttachPoint.Spine0, AttachPoint.HandL);
            DrawToggleGroup(AttachPoint.ThighR, AttachPoint.Pelvis, AttachPoint.ThighL);
            DrawToggleGroup(AttachPoint.CalfR, AttachPoint.CalfL);
            DrawToggleGroup(AttachPoint.FootR, AttachPoint.FootL);

            GUI.enabled = true;
        }

        private void DrawToggleGroup(params AttachPoint[] attachPoints)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var point in attachPoints) toggles[point].Draw();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnToggleChange(AttachPoint point)
        {
            if (updating) return;

            var toggle = toggles[point];
            if (toggle.Value)
            {
                if (activeToggle != null)
                {
                    updating = true;
                    activeToggle.Value = false;
                    updating = false;
                }

                activeToggle = toggle;
                SelectedProp.AttachTo(SelectedMeido, point, KeepWoldPosition);
            }
            else
            {
                SelectedProp.DetachFrom(KeepWoldPosition);
                activeToggle = null;
            }
        }

        private void UpdateToggles()
        {
            updating = true;
            if (activeToggle != null) activeToggle.Value = false;
            activeToggle = null;
            updating = false;

            if (!meidoManager.HasActiveMeido || propManager.PropCount == 0) return;

            var info = SelectedProp.AttachPointInfo;

            if (SelectedMeido.Maid.status.guid != info.MaidGuid) return;

            updating = true;
            var toggle = toggles[info.AttachPoint];
            toggle.Value = true;
            activeToggle = toggle;
            updating = false;
        }

        private void SetMeidoDropdown()
        {
            meidoDropdownActive = meidoManager.HasActiveMeido;
            string[] dropdownList = meidoManager.ActiveMeidoList.Count == 0
                ? new[] { Translation.Get("systemMessage", "noMaids") }
                : meidoManager.ActiveMeidoList.Select(meido => $"{meido.Slot + 1}: {meido.FirstName} {meido.LastName}")
                    .ToArray();

            meidoDropdown.SetDropdownItems(dropdownList, 0);
        }
    }
}
