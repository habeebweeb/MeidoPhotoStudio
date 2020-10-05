using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragPointPane : BasePane
    {
        private string header;
        private readonly Toggle propsCubeToggle;
        private readonly Toggle smallCubeToggle;
        private readonly Toggle maidCubeToggle;
        private readonly Toggle bgCubeToggle;
        private enum Setting
        {
            Prop, Maid, Background, Size
        }

        public DragPointPane()
        {
            header = Translation.Get("movementCube", "header");
            propsCubeToggle = new Toggle(Translation.Get("movementCube", "props"), PropManager.CubeActive);
            smallCubeToggle = new Toggle(Translation.Get("movementCube", "small"));
            maidCubeToggle = new Toggle(Translation.Get("movementCube", "maid"), MeidoDragPointManager.CubeActive);
            bgCubeToggle = new Toggle(Translation.Get("movementCube", "bg"), EnvironmentManager.CubeActive);

            propsCubeToggle.ControlEvent += (s, a) => ChangeDragPointSetting(Setting.Prop, propsCubeToggle.Value);
            smallCubeToggle.ControlEvent += (s, a) => ChangeDragPointSetting(Setting.Size, smallCubeToggle.Value);
            maidCubeToggle.ControlEvent += (s, a) => ChangeDragPointSetting(Setting.Maid, maidCubeToggle.Value);
            bgCubeToggle.ControlEvent += (s, a) => ChangeDragPointSetting(Setting.Background, bgCubeToggle.Value);
        }

        protected override void ReloadTranslation()
        {
            header = Translation.Get("movementCube", "header");
            propsCubeToggle.Label = Translation.Get("movementCube", "props");
            smallCubeToggle.Label = Translation.Get("movementCube", "small");
            maidCubeToggle.Label = Translation.Get("movementCube", "maid");
            bgCubeToggle.Label = Translation.Get("movementCube", "bg");
        }

        public override void Draw()
        {
            MpsGui.Header(header);
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            propsCubeToggle.Draw();
            smallCubeToggle.Draw();
            maidCubeToggle.Draw();
            bgCubeToggle.Draw();
            GUILayout.EndHorizontal();
        }

        private void ChangeDragPointSetting(Setting setting, bool value)
        {
            switch (setting)
            {
                case Setting.Prop:
                    PropManager.CubeActive = value;
                    break;
                case Setting.Background:
                    EnvironmentManager.CubeActive = value;
                    break;
                case Setting.Maid:
                    MeidoDragPointManager.CubeActive = value;
                    break;
                case Setting.Size:
                    MeidoDragPointManager.CubeSmall = value;
                    EnvironmentManager.CubeSmall = value;
                    PropManager.CubeSmall = value;
                    break;
            }
        }
    }
}
