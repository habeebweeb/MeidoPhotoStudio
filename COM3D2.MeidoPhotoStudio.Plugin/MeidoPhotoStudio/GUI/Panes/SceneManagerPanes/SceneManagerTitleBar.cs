using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneManagerTitleBarPane : BasePane
    {
        public event System.EventHandler closeChange;
        private SceneManager sceneManager;
        private Button kankyoToggle;
        private Button refreshButton;
        private Dropdown sortDropdown;
        private Toggle descendingToggle;
        private Button closeButton;
        private string sortLabel;

        public SceneManagerTitleBarPane(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
            kankyoToggle = new Button("Backgrounds");
            kankyoToggle.ControlEvent += (s, a) => sceneManager.ToggleKankyoMode();

            refreshButton = new Button("Refresh");
            refreshButton.ControlEvent += (s, a) => sceneManager.Refresh();

            sortDropdown = new Dropdown(new[] { "Name", "Date Created", "Date Modified" });
            sortDropdown.SelectionChange += (s, a) =>
            {
                SceneManager.SortMode sortMode = (SceneManager.SortMode)sortDropdown.SelectedItemIndex;
                if (sceneManager.CurrentSortMode == sortMode) return;
                sceneManager.SortScenes(sortMode);
            };

            descendingToggle = new Toggle("Descending", sceneManager.SortDescending);
            descendingToggle.ControlEvent += (s, a) =>
            {
                sceneManager.SortDescending = descendingToggle.Value;
                sceneManager.SortScenes(sceneManager.CurrentSortMode);
            };

            closeButton = new Button("X");
            closeButton.ControlEvent += (s, a) => closeChange?.Invoke(this, System.EventArgs.Empty);

            sortLabel = "Sort";
        }

        public override void Draw()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = Utility.GetPix(12);

            GUILayoutOption buttonHeight = GUILayout.Height(Utility.GetPix(20));
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Utility.GetPix(SceneManagerDirectoryPane.listWidth)));

            Color originalColour = GUI.backgroundColor;
            if (sceneManager.KankyoMode) GUI.backgroundColor = Color.green;
            kankyoToggle.Draw(buttonStyle, buttonHeight);
            GUI.backgroundColor = originalColour;

            GUILayout.FlexibleSpace();

            refreshButton.Draw(buttonStyle, buttonHeight);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(Utility.GetPix(15));

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = buttonStyle.fontSize;

            GUILayout.Label(sortLabel, labelStyle);

            GUIStyle dropdownStyle = new GUIStyle(DropdownHelper.DefaultDropdownStyle);
            dropdownStyle.fontSize = buttonStyle.fontSize;

            sortDropdown.Draw(buttonStyle, dropdownStyle, buttonHeight, GUILayout.Width(Utility.GetPix(100)));

            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.fontSize = buttonStyle.fontSize;

            descendingToggle.Draw(toggleStyle);

            GUILayout.FlexibleSpace();

            closeButton.Draw();

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
    }
}
