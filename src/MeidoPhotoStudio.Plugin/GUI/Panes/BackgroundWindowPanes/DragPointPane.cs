using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPane : BasePane
{
    private readonly Toggle propsCubeToggle;
    private readonly Toggle smallCubeToggle;
    private readonly Toggle maidCubeToggle;

    private string header;

    public DragPointPane()
    {
        header = Translation.Get("movementCube", "header");

        propsCubeToggle = new(Translation.Get("movementCube", "props"), PropManager.CubeActive);
        propsCubeToggle.ControlEvent += (_, _) =>
            ChangeDragPointSetting(Setting.Prop, propsCubeToggle.Value);

        smallCubeToggle = new(Translation.Get("movementCube", "small"));
        smallCubeToggle.ControlEvent += (_, _) =>
            ChangeDragPointSetting(Setting.Size, smallCubeToggle.Value);

        maidCubeToggle = new(Translation.Get("movementCube", "maid"), MeidoDragPointManager.CubeActive);
        maidCubeToggle.ControlEvent += (_, _) =>
            ChangeDragPointSetting(Setting.Maid, maidCubeToggle.Value);

        bgCubeToggle = new(Translation.Get("movementCube", "bg"), EnvironmentManager.CubeActive);
        bgCubeToggle.ControlEvent += (_, _) =>
            ChangeDragPointSetting(Setting.Background, bgCubeToggle.Value);
    }

    private enum Setting
    {
        Prop,
        Maid,
        Background,
        Size,
    }

    public override void Draw()
    {
        MpsGui.Header(header);
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        propsCubeToggle.Draw();
        smallCubeToggle.Draw();
        maidCubeToggle.Draw();
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        header = Translation.Get("movementCube", "header");
        propsCubeToggle.Label = Translation.Get("movementCube", "props");
        smallCubeToggle.Label = Translation.Get("movementCube", "small");
        maidCubeToggle.Label = Translation.Get("movementCube", "maid");
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
            default:
                break;
        }
    }
}
