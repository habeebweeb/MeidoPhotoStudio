using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidFaceBlendPane : BasePane
{
    private static readonly string[] TabTranslations = { "baseTab", "customTab" };

    private readonly MeidoManager meidoManager;
    private readonly SelectionGrid faceBlendSourceGrid;
    private readonly Dropdown faceBlendCategoryDropdown;
    private readonly Button prevCategoryButton;
    private readonly Button nextCategoryButton;
    private readonly Dropdown faceBlendDropdown;
    private readonly Button facePrevButton;
    private readonly Button faceNextButton;

    private bool facePresetMode;
    private bool faceListEnabled;

    public MaidFaceBlendPane(MeidoManager meidoManager)
    {
        Constants.CustomFaceChange += OnPresetChange;

        this.meidoManager = meidoManager;

        faceBlendSourceGrid = new(Translation.GetArray("maidFaceWindow", TabTranslations));

        faceBlendSourceGrid.ControlEvent += (_, _) =>
        {
            facePresetMode = faceBlendSourceGrid.SelectedItemIndex is 1;

            if (updating)
                return;

            var list = facePresetMode
                ? CurrentFaceGroupList.ToArray()
                : Translation.GetArray("faceBlendCategory", Constants.FaceGroupList);

            faceBlendCategoryDropdown.SetDropdownItems(list, 0);
        };

        faceBlendCategoryDropdown = new(Translation.GetArray("faceBlendCategory", Constants.FaceGroupList));
        faceBlendCategoryDropdown.SelectionChange += (_, _) =>
        {
            faceListEnabled = CurrentFaceList.Count > 0;
            faceBlendDropdown.SetDropdownItems(UIFaceList(), 0);
        };

        prevCategoryButton = new("<");
        prevCategoryButton.ControlEvent += (_, _) =>
            faceBlendCategoryDropdown.Step(-1);

        nextCategoryButton = new(">");
        nextCategoryButton.ControlEvent += (_, _) =>
            faceBlendCategoryDropdown.Step(1);

        faceBlendDropdown = new(UIFaceList());
        faceBlendDropdown.SelectionChange += (_, _) =>
        {
            if (!faceListEnabled || updating)
                return;

            this.meidoManager.ActiveMeido.SetFaceBlendSet(SelectedFace);
        };

        facePrevButton = new("<");
        facePrevButton.ControlEvent += (_, _) =>
            faceBlendDropdown.Step(-1);

        faceNextButton = new(">");
        faceNextButton.ControlEvent += (_, _) =>
            faceBlendDropdown.Step(1);

        faceListEnabled = CurrentFaceList.Count > 0;
    }

    private Dictionary<string, List<string>> CurrentFaceDict =>
        facePresetMode ? Constants.CustomFaceDict : Constants.FaceDict;

    private List<string> CurrentFaceGroupList =>
        facePresetMode ? Constants.CustomFaceGroupList : Constants.FaceGroupList;

    private string SelectedFaceGroup =>
        CurrentFaceGroupList[faceBlendCategoryDropdown.SelectedItemIndex];

    private List<string> CurrentFaceList =>
        CurrentFaceDict[SelectedFaceGroup];

    private int SelectedFaceIndex =>
        faceBlendDropdown.SelectedItemIndex;

    private string SelectedFace =>
        CurrentFaceList[SelectedFaceIndex];

    public override void Draw()
    {
        const float buttonHeight = 30;

        var arrowLayoutOptions = new[]
        {
            GUILayout.Width(buttonHeight),
            GUILayout.Height(buttonHeight),
        };

        const float dropdownButtonWidth = 153f;

        var dropdownLayoutOptions = new[]
        {
            GUILayout.Height(buttonHeight),
            GUILayout.Width(dropdownButtonWidth),
        };

        GUI.enabled = meidoManager.HasActiveMeido;

        faceBlendSourceGrid.Draw();

        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        prevCategoryButton.Draw(arrowLayoutOptions);
        faceBlendCategoryDropdown.Draw(dropdownLayoutOptions);
        nextCategoryButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.enabled = GUI.enabled && faceListEnabled;
        facePrevButton.Draw(arrowLayoutOptions);
        faceBlendDropdown.Draw(dropdownLayoutOptions);
        faceNextButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        updating = true;
        faceBlendSourceGrid.SetItems(Translation.GetArray("maidFaceWindow", TabTranslations));

        if (!facePresetMode)
            faceBlendCategoryDropdown.SetDropdownItems(
                Translation.GetArray("faceBlendCategory", Constants.FaceGroupList));

        updating = false;
    }

    private string[] UIFaceList() =>
        CurrentFaceList.Count is 0
            ? new[] { "No Face Presets" }
            : CurrentFaceList.Select(face => facePresetMode
                ? Path.GetFileNameWithoutExtension(face)
                : Translation.Get("faceBlendPresetsDropdown", face)).ToArray();

    private void OnPresetChange(object sender, PresetChangeEventArgs args)
    {
        if (args == PresetChangeEventArgs.Empty)
        {
            if (facePresetMode)
            {
                updating = true;
                faceBlendCategoryDropdown.SetDropdownItems(CurrentFaceGroupList.ToArray(), 0);
                faceBlendDropdown.SetDropdownItems(UIFaceList(), 0);
                updating = false;
            }
        }
        else
        {
            updating = true;
            faceBlendSourceGrid.SelectedItemIndex = 1;
            faceBlendCategoryDropdown.SetDropdownItems(
                CurrentFaceGroupList.ToArray(), CurrentFaceGroupList.IndexOf(args.Category));

            updating = false;
            faceBlendDropdown.SetDropdownItems(UIFaceList(), CurrentFaceList.IndexOf(args.Path));
        }
    }
}
