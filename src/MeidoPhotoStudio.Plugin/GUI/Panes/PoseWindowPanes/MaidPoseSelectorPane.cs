using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidPoseSelectorPane : BasePane
{
    private static readonly string[] TabTranslations = new[] { "baseTab", "customTab" };

    private readonly MeidoManager meidoManager;
    private readonly Button poseLeftButton;
    private readonly Button poseRightButton;
    private readonly Button poseGroupLeftButton;
    private readonly Button poseGroupRightButton;
    private readonly Dropdown poseGroupDropdown;
    private readonly Dropdown poseDropdown;
    private readonly SelectionGrid poseModeGrid;

    private bool customPoseMode;
    private bool poseListEnabled;
    private string previousPoseGroup;

    public MaidPoseSelectorPane(MeidoManager meidoManager)
    {
        Constants.CustomPoseChange += OnPresetChange;
        this.meidoManager = meidoManager;

        poseModeGrid = new(Translation.GetArray("posePane", TabTranslations));
        poseModeGrid.ControlEvent += (_, _) =>
            SetPoseMode();

        poseGroupDropdown = new(Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList));
        poseGroupDropdown.SelectionChange += (_, _) =>
            ChangePoseGroup();

        poseDropdown = new(UIPoseList());
        poseDropdown.SelectionChange += (_, _) =>
            ChangePose();

        poseGroupLeftButton = new("<");
        poseGroupLeftButton.ControlEvent += (_, _) =>
            poseGroupDropdown.Step(-1);

        poseGroupRightButton = new(">");
        poseGroupRightButton.ControlEvent += (_, _) =>
            poseGroupDropdown.Step(1);

        poseLeftButton = new("<");
        poseLeftButton.ControlEvent += (_, _) =>
            poseDropdown.Step(-1);

        poseRightButton = new(">");
        poseRightButton.ControlEvent += (_, _) =>
            poseDropdown.Step(1);

        customPoseMode = poseModeGrid.SelectedItemIndex is 1;
        previousPoseGroup = SelectedPoseGroup;
        poseListEnabled = CurrentPoseList.Count > 0;
    }

    private Dictionary<string, List<string>> CurrentPoseDict =>
        customPoseMode ? Constants.CustomPoseDict : Constants.PoseDict;

    private List<string> CurrentPoseGroupList =>
        customPoseMode ? Constants.CustomPoseGroupList : Constants.PoseGroupList;

    private string SelectedPoseGroup =>
        CurrentPoseGroupList[poseGroupDropdown.SelectedItemIndex];

    private List<string> CurrentPoseList =>
        CurrentPoseDict[SelectedPoseGroup];

    private int SelectedPoseIndex =>
        poseDropdown.SelectedItemIndex;

    private string SelectedPose =>
        CurrentPoseList[SelectedPoseIndex];

    private PoseInfo CurrentPoseInfo =>
        new(SelectedPoseGroup, SelectedPose, customPoseMode);

    public override void Draw()
    {
        const float buttonHeight = 30f;

        var arrowLayoutOptions = new[]
        {
            GUILayout.Width(buttonHeight),
            GUILayout.Height(buttonHeight),
        };

        const float dropdownButtonWidth = 153f;

        var dropdownLayoutOptions = new GUILayoutOption[]
        {
            GUILayout.Height(buttonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        GUI.enabled = meidoManager.HasActiveMeido && !meidoManager.ActiveMeido.Stop;

        poseModeGrid.Draw();
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        poseGroupLeftButton.Draw(arrowLayoutOptions);
        poseGroupDropdown.Draw(dropdownLayoutOptions);
        poseGroupRightButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.enabled = GUI.enabled && poseListEnabled;
        poseLeftButton.Draw(arrowLayoutOptions);
        poseDropdown.Draw(dropdownLayoutOptions);
        poseRightButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    public override void UpdatePane()
    {
        updating = true;

        try
        {
            var cachedPose = meidoManager.ActiveMeido.CachedPose;

            poseModeGrid.SelectedItemIndex = cachedPose.CustomPose ? 1 : 0;

            var oldCustomPoseMode = customPoseMode;
            customPoseMode = cachedPose.CustomPose;

            if (oldCustomPoseMode != customPoseMode)
                poseGroupDropdown.SetDropdownItems(
                    customPoseMode
                        ? CurrentPoseGroupList.ToArray()
                        : Translation.GetArray("poseGroupDropdown", CurrentPoseGroupList));

            var newPoseGroupIndex = CurrentPoseGroupList.IndexOf(cachedPose.PoseGroup);

            if (newPoseGroupIndex < 0)
            {
                poseGroupDropdown.SelectedItemIndex = 0;
            }
            else if (oldCustomPoseMode != customPoseMode || poseGroupDropdown.SelectedItemIndex != newPoseGroupIndex)
            {
                poseGroupDropdown.SelectedItemIndex = newPoseGroupIndex;
                poseDropdown.SetDropdownItems(UIPoseList());
            }

            var newPoseIndex = CurrentPoseDict.TryGetValue(cachedPose.PoseGroup, out var poseList)
                ? poseList.IndexOf(cachedPose.Pose)
                : 0;

            if (newPoseIndex < 0)
                newPoseIndex = 0;

            poseDropdown.SelectedItemIndex = newPoseIndex;
            poseListEnabled = CurrentPoseList.Count > 0;
        }
        catch
        {
            // Ignored
        }
        finally
        {
            updating = false;
        }
    }

    protected override void ReloadTranslation()
    {
        updating = true;
        poseModeGrid.SetItems(Translation.GetArray("posePane", TabTranslations));

        if (!customPoseMode)
            poseGroupDropdown.SetDropdownItems(Translation.GetArray("poseGroupDropdown", Constants.PoseGroupList));

        updating = false;
    }

    private void OnPresetChange(object sender, PresetChangeEventArgs args)
    {
        if (args == PresetChangeEventArgs.Empty)
        {
            if (poseModeGrid.SelectedItemIndex is 1)
            {
                updating = true;
                poseGroupDropdown.SetDropdownItems(CurrentPoseGroupList.ToArray(), 0);
                poseDropdown.SetDropdownItems(UIPoseList(), 0);
                updating = false;
            }
        }
        else
        {
            updating = true;
            poseModeGrid.SelectedItemIndex = 1;
            poseGroupDropdown.SetDropdownItems(
                CurrentPoseGroupList.ToArray(), CurrentPoseGroupList.IndexOf(args.Category));

            updating = false;

            poseDropdown.SetDropdownItems(UIPoseList(), CurrentPoseList.IndexOf(args.Path));
            poseListEnabled = true;
        }
    }

    private void SetPoseMode()
    {
        if (updating)
            return;

        customPoseMode = poseModeGrid.SelectedItemIndex is 1;

        var list = customPoseMode
            ? CurrentPoseGroupList.ToArray()
            : Translation.GetArray("poseGroupDropdown", CurrentPoseGroupList);

        poseGroupDropdown.SetDropdownItems(list, 0);
    }

    private void ChangePoseGroup()
    {
        if (updating)
            return;

        poseListEnabled = CurrentPoseList.Count > 0;

        if (previousPoseGroup == SelectedPoseGroup)
        {
            poseDropdown.SelectedItemIndex = 0;
        }
        else
        {
            previousPoseGroup = SelectedPoseGroup;
            poseDropdown.SetDropdownItems(UIPoseList(), 0);
        }
    }

    private void ChangePose()
    {
        if (!poseListEnabled || updating)
            return;

        meidoManager.ActiveMeido.SetPose(CurrentPoseInfo);
    }

    private string[] UIPoseList() =>
        CurrentPoseList.Count is 0
            ? new[] { "No Poses" }
            : CurrentPoseList
                .Select((pose, i) => $"{i + 1}:{(customPoseMode ? Path.GetFileNameWithoutExtension(pose) : pose)}")
                .ToArray();
}
