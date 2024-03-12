using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Props;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class AttachPropPane : BasePane
{
    private static readonly Dictionary<AttachPoint, string> ToggleTranslation =
        new()
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
            [AttachPoint.Spine0] = "spine0",
        };

    private readonly PropService propService;
    private readonly PropAttachmentService propAttachmentService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly MeidoManager meidoManager;
    private readonly Dictionary<AttachPoint, Toggle> toggles = new(EnumEqualityComparer<AttachPoint>.Instance);
    private readonly Toggle keepWorldPositionToggle;
    private readonly Dropdown meidoDropdown;
    private readonly Toggle paneHeader;

    private bool meidoDropdownActive;
    private bool doguDropdownActive;

    public AttachPropPane(
        MeidoManager meidoManager,
        PropService propService,
        PropAttachmentService propAttachmentService,
        SelectionController<PropController> propSelectionController)
    {
        this.meidoManager = meidoManager;
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propAttachmentService = propAttachmentService ?? throw new ArgumentNullException(nameof(propAttachmentService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));

        this.propService.AddedProp += OnPropAddedOrRemoved;
        this.propService.RemovedProp += OnPropAddedOrRemoved;

        this.meidoManager.EndCallMeidos += (_, _) =>
            SetMeidoDropdown();

        this.propSelectionController.Selected += OnPropOrMaidChanged;

        meidoDropdown = new(new[] { Translation.Get("systemMessage", "noMaids") });
        meidoDropdown.SelectionChange += OnPropOrMaidChanged;

        keepWorldPositionToggle = new(Translation.Get("attachPropPane", "keepWorldPosition"));

        foreach (var attachPoint in Enum.GetValues(typeof(AttachPoint)).Cast<AttachPoint>())
        {
            if (attachPoint is AttachPoint.None)
                continue;

            var point = attachPoint;
            var toggle = new Toggle(Translation.Get("attachPropPane", ToggleTranslation[point]));

            toggle.ControlEvent += (_, _) =>
                OnToggleChanged(point);

            toggles[point] = toggle;
        }

        paneHeader = new(Translation.Get("attachPropPane", "header"), true);
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    private bool PaneActive =>
        meidoDropdownActive && doguDropdownActive;

    private Meido SelectedMeido =>
        meidoManager.HasActiveMeido
            ? meidoManager.ActiveMeidoList[meidoDropdown.SelectedItemIndex]
            : null;

    public override void Draw()
    {
        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        GUI.enabled = PaneActive;

        DrawMeidoDropdown();

        MpsGui.BlackLine();

        keepWorldPositionToggle.Draw();

        MpsGui.BlackLine();

        DrawAttachPointToggles();

        GUI.enabled = true;

        void DrawMeidoDropdown()
        {
            GUILayout.BeginHorizontal();
            meidoDropdown.Draw(GUILayout.Width(185f));

            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            if (GUILayout.Button("<", arrowLayoutOptions))
                meidoDropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                meidoDropdown.Step(1);

            GUILayout.EndHorizontal();
        }

        void DrawAttachPointToggles()
        {
            DrawToggleGroup(AttachPoint.Head, AttachPoint.Neck);
            DrawToggleGroup(AttachPoint.UpperArmR, AttachPoint.Spine1a, AttachPoint.UpperArmL);
            DrawToggleGroup(AttachPoint.ForearmR, AttachPoint.Spine1, AttachPoint.ForearmL);
            DrawToggleGroup(AttachPoint.MuneR, AttachPoint.Spine0a, AttachPoint.MuneL);
            DrawToggleGroup(AttachPoint.HandR, AttachPoint.Spine0, AttachPoint.HandL);
            DrawToggleGroup(AttachPoint.ThighR, AttachPoint.Pelvis, AttachPoint.ThighL);
            DrawToggleGroup(AttachPoint.CalfR, AttachPoint.CalfL);
            DrawToggleGroup(AttachPoint.FootR, AttachPoint.FootL);
        }
    }

    public override void UpdatePane()
    {
        base.UpdatePane();

        UpdateToggles();
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("attachPropPane", "header");
        keepWorldPositionToggle.Label = Translation.Get("attachPropPane", "keepWorldPosition");

        foreach (var attachPoint in Enum.GetValues(typeof(AttachPoint)).Cast<AttachPoint>())
        {
            if (attachPoint is AttachPoint.None)
                continue;

            toggles[attachPoint].Label = Translation.Get("attachPropPane", ToggleTranslation[attachPoint]);
        }
    }

    private void DrawToggleGroup(params AttachPoint[] attachPoints)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        foreach (var point in attachPoints)
            toggles[point].Draw();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void OnToggleChanged(AttachPoint point)
    {
        if (CurrentProp is null || SelectedMeido is null)
            return;

        var changedToggle = toggles[point];

        if (changedToggle.Value)
        {
            propAttachmentService.AttachPropTo(CurrentProp, SelectedMeido, point, keepWorldPositionToggle.Value);

            var otherEnabledToggles = toggles.Values
                .Where(toggle => toggle != changedToggle)
                .Where(toggle => toggle.Value);

            foreach (var toggle in otherEnabledToggles)
                toggle.SetEnabledWithoutNotify(false);
        }
        else
        {
            propAttachmentService.DetachProp(CurrentProp);
        }
    }

    private void UpdateToggles()
    {
        if (CurrentProp is null || SelectedMeido is null)
            return;

        if (propAttachmentService.TryGetAttachPointInfo(CurrentProp, out var attachPointInfo))
        {
            var attachedToggle = toggles[attachPointInfo.AttachPoint];

            attachedToggle.SetEnabledWithoutNotify(SelectedMeido.Maid.status.guid == attachPointInfo.MaidGuid);

            var otherEnabledToggles = toggles.Values
                .Where(toggle => toggle != attachedToggle)
                .Where(toggle => toggle.Value);

            foreach (var toggle in otherEnabledToggles)
                toggle.SetEnabledWithoutNotify(false);
        }
        else
        {
            foreach (var toggle in toggles.Values.Where(toggle => toggle.Value))
                toggle.SetEnabledWithoutNotify(false);
        }
    }

    private void SetMeidoDropdown()
    {
        meidoDropdownActive = meidoManager.HasActiveMeido;

        var dropdownList = meidoManager.ActiveMeidoList.Count is 0
            ? new[] { Translation.Get("systemMessage", "noMaids") }
            : meidoManager.ActiveMeidoList.Select(meido => $"{meido.Slot + 1}: {meido.FirstName} {meido.LastName}")
                .ToArray();

        meidoDropdown.SetDropdownItems(dropdownList, 0);
    }

    private void OnPropOrMaidChanged(object sender, EventArgs e) =>
        UpdateToggles();

    private void OnPropAddedOrRemoved(object sender, PropServiceEventArgs e)
    {
        doguDropdownActive = propService.Count > 0;

        UpdateToggles();
    }
}
