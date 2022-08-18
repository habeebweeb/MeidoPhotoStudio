using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class TabsPane : BasePane
{
    private static readonly string[] TabNames = { "call", "pose", "face", "bg", "bg2" };

    private readonly SelectionGrid tabs;

    private Constants.Window selectedTab;

    public TabsPane()
    {
        Translation.ReloadTranslationEvent += (_, _) =>
            ReloadTranslation();

        tabs = new(Translation.GetArray("tabs", TabNames));
        tabs.ControlEvent += (_, _) =>
            OnChangeTab();
    }

    public event EventHandler TabChange;

    public Constants.Window SelectedTab
    {
        get => selectedTab;
        set => tabs.SelectedItemIndex = (int)value;
    }

    public override void Draw()
    {
        tabs.Draw(GUILayout.ExpandWidth(false));
        MpsGui.BlackLine();
    }

    protected override void ReloadTranslation()
    {
        updating = true;
        tabs.SetItems(Translation.GetArray("tabs", TabNames), tabs.SelectedItemIndex);
        updating = false;
    }

    private void OnChangeTab()
    {
        if (updating)
            return;

        selectedTab = (Constants.Window)tabs.SelectedItemIndex;
        TabChange?.Invoke(null, EventArgs.Empty);
    }
}
