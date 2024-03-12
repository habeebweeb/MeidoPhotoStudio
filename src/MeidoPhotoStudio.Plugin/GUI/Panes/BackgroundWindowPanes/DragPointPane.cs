using System;

using MeidoPhotoStudio.Plugin.Core.Props;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPane : BasePane
{
    private readonly Toggle smallCubeToggle;
    private readonly Toggle maidCubeToggle;
    private readonly PropDragHandleService propDragHandleService;

    private string header;

    public DragPointPane(PropDragHandleService propDragHandleService)
    {
        this.propDragHandleService = propDragHandleService;

        header = Translation.Get("movementCube", "header");

        smallCubeToggle = new(Translation.Get("movementCube", "small"));
        smallCubeToggle.ControlEvent += OnSmallCubeToggleChanged;

        maidCubeToggle = new(Translation.Get("movementCube", "maid"), MeidoDragPointManager.CubeActive);
        maidCubeToggle.ControlEvent += OnMaidCubeToggleChanged;
    }

    public override void Draw()
    {
        MpsGui.Header(header);
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        smallCubeToggle.Draw();
        maidCubeToggle.Draw();
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        header = Translation.Get("movementCube", "header");
        smallCubeToggle.Label = Translation.Get("movementCube", "small");
        maidCubeToggle.Label = Translation.Get("movementCube", "maid");
    }

    private void OnSmallCubeToggleChanged(object sender, EventArgs e)
    {
        var toggle = (Toggle)sender;

        MeidoDragPointManager.CubeSmall = toggle.Value;
        propDragHandleService.SmallHandle = toggle.Value;
    }

    private void OnMaidCubeToggleChanged(object sender, EventArgs e) =>
        MeidoDragPointManager.CubeActive = ((Toggle)sender).Value;
}
