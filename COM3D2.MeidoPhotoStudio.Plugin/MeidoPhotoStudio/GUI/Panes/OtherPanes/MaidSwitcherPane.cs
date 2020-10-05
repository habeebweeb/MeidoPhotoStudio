using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidSwitcherPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Button previousButton;
        private readonly Button nextButton;
        private readonly Toggle editToggle;

        public MaidSwitcherPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.meidoManager.UpdateMeido += (s, a) => UpdatePane();

            previousButton = new Button("<");
            previousButton.ControlEvent += (s, a) => ChangeMaid(-1);

            nextButton = new Button(">");
            nextButton.ControlEvent += (s, a) => ChangeMaid(1);

            editToggle = new Toggle("Edit", true);
            editToggle.ControlEvent += (s, a) => SetEditMaid();
        }

        public override void Draw()
        {
            const float boxSize = 70;
            const int margin = (int)(boxSize / 2.8f);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin.top = margin;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.margin.top = margin;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box) { margin = new RectOffset(0, 0, 0, 0) };
            GUIStyle horizontalStyle = new GUIStyle { padding = new RectOffset(4, 4, 0, 0) };

            GUILayoutOption[] buttonOptions = new[] { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false) };
            GUILayoutOption[] boxLayoutOptions = new[] { GUILayout.Height(boxSize), GUILayout.Width(boxSize) };

            GUI.enabled = meidoManager.HasActiveMeido;
            Meido meido = meidoManager.ActiveMeido;

            GUILayout.BeginHorizontal(horizontalStyle, GUILayout.Height(boxSize));

            previousButton.Draw(buttonStyle, buttonOptions);

            GUILayout.Space(20);

            if (meidoManager.HasActiveMeido && meido.Portrait) MpsGui.DrawTexture(meido.Portrait, boxLayoutOptions);
            else GUILayout.Box(GUIContent.none, boxStyle, boxLayoutOptions);

            string label = meidoManager.HasActiveMeido ? $"{meido.LastName}\n{meido.FirstName}" : string.Empty;

            GUILayout.Label(label, labelStyle, GUILayout.ExpandWidth(false));

            GUILayout.FlexibleSpace();

            nextButton.Draw(buttonStyle, buttonOptions);

            GUILayout.EndHorizontal();

            Rect previousRect = GUILayoutUtility.GetLastRect();

            if (MeidoPhotoStudio.EditMode) editToggle.Draw(new Rect(previousRect.x + 4f, previousRect.y, 40f, 20f));

            Rect labelRect = new Rect(previousRect.width - 45f, previousRect.y, 40f, 20f);
            GUIStyle slotStyle = new GUIStyle()
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 13
            };
            slotStyle.padding.right = 5;
            slotStyle.normal.textColor = Color.white;

            if (meidoManager.HasActiveMeido) GUI.Label(labelRect, $"{meidoManager.ActiveMeido.Slot + 1}", slotStyle);
        }

        public override void UpdatePane()
        {
            if (meidoManager.HasActiveMeido)
            {
                this.updating = true;
                editToggle.Value = meidoManager.ActiveMeido.IsEditMaid;
                this.updating = false;
            }
        }

        private void ChangeMaid(int dir)
        {
            int selected = Utility.Wrap(
                meidoManager.SelectedMeido + (int)Mathf.Sign(dir), 0, meidoManager.ActiveMeidoList.Count
            );

            meidoManager.ChangeMaid(selected);
        }

        private void SetEditMaid()
        {
            if (updating) return;

            if (!editToggle.Value)
            {
                updating = true;
                editToggle.Value = true;
                updating = false;
                return;
            }

            if (meidoManager.HasActiveMeido)
            {
                meidoManager.SetEditMaid(meidoManager.ActiveMeido);
            }
        }
    }
}
