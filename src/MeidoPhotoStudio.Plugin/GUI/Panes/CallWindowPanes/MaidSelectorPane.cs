using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Core;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidSelectorPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Button clearMaidsButton;
    private readonly Button callMaidsButton;
    private readonly Toggle activeMeidoListToggle;
    private readonly List<Meido> selectedMeidoList = new();
    private readonly HashSet<Meido> selectedMeidoSet = new();

    private Vector2 maidListScrollPos;
    private Vector2 activeMaidListScrollPos;

    public MaidSelectorPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        clearMaidsButton = new(Translation.Get("maidCallWindow", "clearButton"));
        clearMaidsButton.ControlEvent += (_, _) =>
            ClearSelectedMaids();

        callMaidsButton = new(Translation.Get("maidCallWindow", "callButton"));
        callMaidsButton.ControlEvent += (_, _) =>
            this.meidoManager.CallMeidos(selectedMeidoList);

        activeMeidoListToggle = new(Translation.Get("maidCallWindow", "activeOnlyToggle"));
        this.meidoManager.BeginCallMeidos += (_, _) =>
        {
            if (selectedMeidoSet.Count is 0)
                activeMeidoListToggle.Value = false;
        };
    }

    public override void Activate()
    {
        base.Activate();

        ClearSelectedMaids();

        // NOTE: Leaving this mode enabled pretty much softlocks meido selection so disable it on activation
        activeMeidoListToggle.Value = false;
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();
        clearMaidsButton.Draw(GUILayout.ExpandWidth(false));
        callMaidsButton.Draw();
        GUILayout.EndHorizontal();

        MpsGui.WhiteLine();

        GUI.enabled = meidoManager.HasActiveMeido;

        activeMeidoListToggle.Draw();

        GUI.enabled = true;

        var onlyActiveMeido = activeMeidoListToggle.Value;

        IList<Meido> meidoList = onlyActiveMeido
            ? meidoManager.ActiveMeidoList
            : meidoManager.Meidos;

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
        };

        var selectLabelStyle = new GUIStyle(labelStyle)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.UpperRight,
        };

        var labelSelectedStyle = new GUIStyle(labelStyle)
        {
            normal = { textColor = Color.black },
        };

        var windowRect = parent.WindowRect;
        var windowHeight = windowRect.height;
        var buttonWidth = windowRect.width - 30f;

        const float buttonHeight = 85f;
        const float offsetTop = 130f;

        var positionRect = new Rect(5f, offsetTop, windowRect.width - 10f, windowHeight - (offsetTop + 35));
        var viewRect = new Rect(0f, 0f, buttonWidth, buttonHeight * meidoList.Count + 5f);

        if (onlyActiveMeido)
            activeMaidListScrollPos = GUI.BeginScrollView(positionRect, activeMaidListScrollPos, viewRect);
        else
            maidListScrollPos = GUI.BeginScrollView(positionRect, maidListScrollPos, viewRect);

        for (var i = 0; i < meidoList.Count; i++)
        {
            var meido = meidoList[i];
            var y = i * buttonHeight;
            var selected = selectedMeidoSet.Contains(meido);

            if (GUI.Button(new(0f, y, buttonWidth, buttonHeight), string.Empty))
                SelectMaid(meido);

            if (selected)
            {
                var selectedIndex = selectedMeidoList.IndexOf(meido) + 1;

                GUI.DrawTexture(new(5f, y + 5f, buttonWidth - 10f, buttonHeight - 10f), Texture2D.whiteTexture);

                GUI.Label(new(0f, y + 5f, buttonWidth - 10f, buttonHeight), selectedIndex.ToString(), selectLabelStyle);
            }

            if (meido.Portrait)
                GUI.DrawTexture(new(5f, y, buttonHeight, buttonHeight), meido.Portrait);

            GUI.Label(
                new(95f, y + 30f, buttonWidth - 80f, buttonHeight),
                $"{meido.LastName}\n{meido.FirstName}",
                selected ? labelSelectedStyle : labelStyle);
        }

        GUI.EndScrollView();
    }

    protected override void ReloadTranslation()
    {
        clearMaidsButton.Label = Translation.Get("maidCallWindow", "clearButton");
        callMaidsButton.Label = Translation.Get("maidCallWindow", "callButton");
        activeMeidoListToggle.Label = Translation.Get("maidCallWindow", "activeOnlyToggle");
    }

    private void SelectMaid(Meido meido)
    {
        if (selectedMeidoSet.Contains(meido))
        {
            if (!PluginCore.EditMode || meido != meidoManager.OriginalEditingMeido)
            {
                selectedMeidoSet.Remove(meido);
                selectedMeidoList.Remove(meido);
            }
        }
        else
        {
            selectedMeidoSet.Add(meido);
            selectedMeidoList.Add(meido);
        }
    }

    private void ClearSelectedMaids()
    {
        selectedMeidoSet.Clear();
        selectedMeidoList.Clear();

        if (!PluginCore.EditMode)
            return;

        SelectMaid(meidoManager.OriginalEditingMeido);
    }
}
