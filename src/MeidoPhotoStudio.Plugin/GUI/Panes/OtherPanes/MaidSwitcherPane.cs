using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MaidSwitcherPane : BasePane
{
    private readonly MeidoManager meidoManager;
    private readonly Button previousButton;
    private readonly Button nextButton;
    private readonly Toggle editToggle;

    public MaidSwitcherPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        this.meidoManager.UpdateMeido += (_, _) =>
            UpdatePane();

        previousButton = new("<");

        previousButton.ControlEvent += (_, _) =>
            ChangeMaid(-1);

        nextButton = new(">");

        nextButton.ControlEvent += (_, _) =>
            ChangeMaid(1);

        editToggle = new("Edit", true);

        editToggle.ControlEvent += (_, _) =>
            SetEditMaid();
    }

    public override void Draw()
    {
        const float boxSize = 70;
        const int margin = (int)(boxSize / 2.8f);

        var buttonStyle = new GUIStyle(GUI.skin.button);

        buttonStyle.margin.top = margin;

        var labelStyle = new GUIStyle(GUI.skin.label);

        labelStyle.margin.top = margin;

        var boxStyle = new GUIStyle(GUI.skin.box)
        {
            margin = new RectOffset(0, 0, 0, 0),
        };

        var horizontalStyle = new GUIStyle
        {
            padding = new RectOffset(4, 4, 0, 0),
        };

        var buttonOptions = new[]
        {
            GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false),
        };

        var boxLayoutOptions = new[]
        {
            GUILayout.Height(boxSize), GUILayout.Width(boxSize),
        };

        GUI.enabled = meidoManager.HasActiveMeido;

        var meido = meidoManager.ActiveMeido;

        GUILayout.BeginHorizontal(horizontalStyle, GUILayout.Height(boxSize));

        previousButton.Draw(buttonStyle, buttonOptions);

        GUILayout.Space(20);

        if (meidoManager.HasActiveMeido && meido.Portrait)
            MpsGui.DrawTexture(meido.Portrait, boxLayoutOptions);
        else
            GUILayout.Box(GUIContent.none, boxStyle, boxLayoutOptions);

        var label = meidoManager.HasActiveMeido ? $"{meido.LastName}\n{meido.FirstName}" : string.Empty;

        GUILayout.Label(label, labelStyle, GUILayout.ExpandWidth(false));

        GUILayout.FlexibleSpace();

        nextButton.Draw(buttonStyle, buttonOptions);

        GUILayout.EndHorizontal();

        var previousRect = GUILayoutUtility.GetLastRect();

        if (MeidoPhotoStudio.EditMode)
            editToggle.Draw(new Rect(previousRect.x + 4f, previousRect.y, 40f, 20f));

        var labelRect = new Rect(previousRect.width - 45f, previousRect.y, 40f, 20f);

        var slotStyle = new GUIStyle()
        {
            alignment = TextAnchor.UpperRight,
            fontSize = 13,
        };

        slotStyle.padding.right = 5;
        slotStyle.normal.textColor = Color.white;

        if (meidoManager.HasActiveMeido)
            GUI.Label(labelRect, $"{meidoManager.ActiveMeido.Slot + 1}", slotStyle);
    }

    public override void UpdatePane()
    {
        if (!meidoManager.HasActiveMeido)
            return;

        updating = true;
        editToggle.Value = meidoManager.ActiveMeido.IsEditMaid;
        updating = false;
    }

    private void ChangeMaid(int dir)
    {
        var selected =
            Utility.Wrap(meidoManager.SelectedMeido + (int)Mathf.Sign(dir), 0, meidoManager.ActiveMeidoList.Count);

        meidoManager.ChangeMaid(selected);
    }

    private void SetEditMaid()
    {
        if (updating)
            return;

        if (!editToggle.Value)
        {
            updating = true;
            editToggle.Value = true;
            updating = false;

            return;
        }

        if (meidoManager.HasActiveMeido)
            meidoManager.SetEditMaid(meidoManager.ActiveMeido);
    }
}
