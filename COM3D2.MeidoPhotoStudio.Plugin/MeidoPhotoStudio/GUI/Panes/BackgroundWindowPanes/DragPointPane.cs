using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointPane : BasePane
    {
        private string header;
        private Toggle propsCubeToggle;
        private Toggle smallCubeToggle;
        private Toggle maidCubeToggle;
        private Toggle bgCubeToggle;
        private enum DragPointSetting
        {
            Prop, Maid, Background, Size
        };

        public DragPointPane()
        {
            this.header = Translation.Get("movementCube", "header");
            this.propsCubeToggle = new Toggle(Translation.Get("movementCube", "props"), PropManager.CubeActive);
            this.smallCubeToggle = new Toggle(Translation.Get("movementCube", "small"));
            this.maidCubeToggle = new Toggle(Translation.Get("movementCube", "maid"), MeidoDragPointManager.CubeActive);
            this.bgCubeToggle = new Toggle(Translation.Get("movementCube", "bg"), EnvironmentManager.CubeActive);

            this.propsCubeToggle.ControlEvent += (s, a) =>
            {
                ChangeDragPointSetting(DragPointSetting.Prop, this.propsCubeToggle.Value);
            };
            this.smallCubeToggle.ControlEvent += (s, a) =>
            {
                ChangeDragPointSetting(DragPointSetting.Size, this.smallCubeToggle.Value);
            };
            this.maidCubeToggle.ControlEvent += (s, a) =>
            {
                ChangeDragPointSetting(DragPointSetting.Maid, this.maidCubeToggle.Value);
            };
            this.bgCubeToggle.ControlEvent += (s, a) =>
            {
                ChangeDragPointSetting(DragPointSetting.Background, this.bgCubeToggle.Value);
            };
        }

        public override void Draw()
        {
            MiscGUI.Header(header);
            MiscGUI.WhiteLine();

            GUILayout.BeginHorizontal();
            this.propsCubeToggle.Draw();
            this.smallCubeToggle.Draw();
            this.maidCubeToggle.Draw();
            this.bgCubeToggle.Draw();
            GUILayout.EndHorizontal();
        }

        private void ChangeDragPointSetting(DragPointSetting setting, bool value)
        {
            switch (setting)
            {
                case DragPointSetting.Prop:
                    PropManager.CubeActive = value;
                    break;
                case DragPointSetting.Background:
                    EnvironmentManager.CubeActive = value;
                    break;
                case DragPointSetting.Maid:
                    MeidoDragPointManager.CubeActive = value;
                    break;
                case DragPointSetting.Size:
                    MeidoDragPointManager.CubeSmall = value;
                    EnvironmentManager.CubeSmall = value;
                    PropManager.CubeSmall = value;
                    break;
            }
        }
    }
}
