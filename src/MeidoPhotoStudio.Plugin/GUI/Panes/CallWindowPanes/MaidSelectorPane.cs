using System.Collections.Generic;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidSelectorPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Button clearMaidsButton;
    private readonly Button callMaidsButton;
    private readonly Toggle activeMeidoListToggle;

    private Vector2 maidListScrollPos;
    private Vector2 activeMaidListScrollPos;

    public MaidSelectorPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        clearMaidsButton = new(Translation.Get("maidCallWindow", "clearButton"));
        clearMaidsButton.ControlEvent += (_, _) =>
            this.meidoManager.ClearSelectList();

        callMaidsButton = new(Translation.Get("maidCallWindow", "callButton"));
        callMaidsButton.ControlEvent += (_, _) =>
            this.meidoManager.CallMeidos();

        activeMeidoListToggle = new(Translation.Get("maidCallWindow", "activeOnlyToggle"));
        this.meidoManager.BeginCallMeidos += (_, _) =>
        {
            if (meidoManager.SelectedMeidoSet.Count is 0)
                activeMeidoListToggle.Value = false;
        };
    }

    public override void Activate()
    {
        base.Activate();

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
            var selectedMaid = meidoManager.SelectedMeidoSet.Contains(meido.StockNo);

            if (GUI.Button(new(0f, y, buttonWidth, buttonHeight), string.Empty))
                meidoManager.SelectMeido(meido.StockNo);

            if (selectedMaid)
            {
                var selectedIndex = meidoManager.SelectMeidoList.IndexOf(meido.StockNo) + 1;

                GUI.DrawTexture(new(5f, y + 5f, buttonWidth - 10f, buttonHeight - 10f), Texture2D.whiteTexture);

                GUI.Label(new(0f, y + 5f, buttonWidth - 10f, buttonHeight), selectedIndex.ToString(), selectLabelStyle);
            }

            if (meido.Portrait)
                GUI.DrawTexture(new(5f, y, buttonHeight, buttonHeight), meido.Portrait);

            GUI.Label(
                new(95f, y + 30f, buttonWidth - 80f, buttonHeight),
                $"{meido.LastName}\n{meido.FirstName}",
                selectedMaid ? labelSelectedStyle : labelStyle);
        }

        GUI.EndScrollView();
    }

    protected override void ReloadTranslation()
    {
        clearMaidsButton.Label = Translation.Get("maidCallWindow", "clearButton");
        callMaidsButton.Label = Translation.Get("maidCallWindow", "callButton");
        activeMeidoListToggle.Label = Translation.Get("maidCallWindow", "activeOnlyToggle");
    }
}
