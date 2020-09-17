using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneManagerTitleBarPane : BasePane
    {
        private static readonly string[] sortModes = new[] { "sortName", "sortCreated", "sortModified" };
        private readonly SceneManager sceneManager;
        private readonly Button kankyoToggle;
        private readonly Button refreshButton;
        private readonly Dropdown sortDropdown;
        private readonly Toggle descendingToggle;
        private readonly Button closeButton;
        private string sortLabel;
        public event System.EventHandler CloseChange;

        public SceneManagerTitleBarPane(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
            kankyoToggle = new Button(Translation.Get("sceneManager", "kankyoToggle"));
            kankyoToggle.ControlEvent += (s, a) => sceneManager.ToggleKankyoMode();

            refreshButton = new Button(Translation.Get("sceneManager", "refreshButton"));
            refreshButton.ControlEvent += (s, a) => sceneManager.Refresh();

            sortDropdown = new Dropdown(
                Translation.GetArray("sceneManager", sortModes), (int)sceneManager.CurrentSortMode
            );
            sortDropdown.SelectionChange += (s, a) =>
            {
                SceneManager.SortMode sortMode = (SceneManager.SortMode)sortDropdown.SelectedItemIndex;
                if (sceneManager.CurrentSortMode == sortMode) return;
                sceneManager.SortScenes(sortMode);
            };

            descendingToggle = new Toggle(
                Translation.Get("sceneManager", "descendingToggle"), sceneManager.SortDescending
            );
            descendingToggle.ControlEvent += (s, a) =>
            {
                sceneManager.SortDescending = descendingToggle.Value;
                sceneManager.SortScenes(sceneManager.CurrentSortMode);
            };

            closeButton = new Button("X");
            closeButton.ControlEvent += (s, a) => CloseChange?.Invoke(this, System.EventArgs.Empty);

            sortLabel = Translation.Get("sceneManager", "sortLabel");
        }

        protected override void ReloadTranslation()
        {
            kankyoToggle.Label = Translation.Get("sceneManager", "kankyoToggle");
            refreshButton.Label = Translation.Get("sceneManager", "refreshButton");
            sortDropdown.SetDropdownItems(Translation.GetArray("sceneManager", sortModes));
            descendingToggle.Label = Translation.Get("sceneManager", "descendingToggle");
            sortLabel = Translation.Get("sceneManager", "sortLabel");
        }

        public override void Draw()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Utility.GetPix(12) };

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

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = buttonStyle.fontSize };

            GUILayout.Label(sortLabel, labelStyle);

            GUIStyle dropdownStyle = new GUIStyle(DropdownHelper.DefaultDropdownStyle)
            {
                fontSize = buttonStyle.fontSize
            };

            sortDropdown.Draw(buttonStyle, dropdownStyle, buttonHeight, GUILayout.Width(Utility.GetPix(100)));

            GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle) { fontSize = buttonStyle.fontSize };

            descendingToggle.Draw(toggleStyle);

            GUILayout.FlexibleSpace();

            closeButton.Draw();

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
    }
}
