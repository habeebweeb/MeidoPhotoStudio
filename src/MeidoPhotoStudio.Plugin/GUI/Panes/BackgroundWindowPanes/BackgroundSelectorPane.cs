using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundSelectorPane : BasePane
{
    private readonly EnvironmentManager environmentManager;
    private readonly Dropdown bgDropdown;
    private readonly Button prevBGButton;
    private readonly Button nextBGButton;

    public BackgroundSelectorPane(EnvironmentManager environmentManager)
    {
        this.environmentManager = environmentManager;

        var theaterIndex = Constants.BGList.FindIndex(bg => bg == EnvironmentManager.DefaultBg);
        var bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));

        if (Constants.MyRoomCustomBGIndex >= 0)
            bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));

        bgDropdown = new(bgList.ToArray(), theaterIndex);
        bgDropdown.SelectionChange += (_, _) =>
            ChangeBackground();

        prevBGButton = new("<");
        prevBGButton.ControlEvent += (_, _) =>
            bgDropdown.Step(-1);

        nextBGButton = new(">");
        nextBGButton.ControlEvent += (_, _) =>
            bgDropdown.Step(1);
    }

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

        GUILayout.BeginHorizontal();
        prevBGButton.Draw(arrowLayoutOptions);
        bgDropdown.Draw(dropdownLayoutOptions);
        nextBGButton.Draw(arrowLayoutOptions);
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        var bgList = new List<string>(Translation.GetList("bgNames", Constants.BGList));

        if (Constants.MyRoomCustomBGIndex >= 0)
            bgList.AddRange(Constants.MyRoomCustomBGList.Select(kvp => kvp.Value));

        updating = true;
        bgDropdown.SetDropdownItems(bgList.ToArray());
        updating = false;
    }

    private void ChangeBackground()
    {
        if (updating)
            return;

        var selectedIndex = bgDropdown.SelectedItemIndex;
        var isCreative = bgDropdown.SelectedItemIndex >= Constants.MyRoomCustomBGIndex;

        var bg = isCreative
            ? Constants.MyRoomCustomBGList[selectedIndex - Constants.MyRoomCustomBGIndex].Key
            : Constants.BGList[selectedIndex];

        environmentManager.ChangeBackground(bg, isCreative);
    }
}
