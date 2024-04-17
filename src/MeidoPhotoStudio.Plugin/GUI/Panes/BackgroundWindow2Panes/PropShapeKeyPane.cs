using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

public class PropShapeKeyPane(SelectionController<PropController> propSelectionController) : BasePane
{
    private readonly SelectionController<PropController> propSelectionController = propSelectionController
        ?? throw new ArgumentNullException(nameof(propSelectionController));

    private readonly Toggle paneHeader = new(Translation.Get("propShapeKeyPane", "header"), true);

    private ShapeKeyController CurrentShapeKeyController =>
        propSelectionController.Current?.ShapeKeyController;

    public override void Draw()
    {
        var enabled = CurrentShapeKeyController != null;

        if (!enabled)
            return;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        var sliderWidth = GUILayout.Width(parent.WindowRect.width / 2 - 15f);

        foreach (var chunk in CurrentShapeKeyController.Chunk(2))
        {
            GUILayout.BeginHorizontal();

            foreach (var (hashKey, blendValue) in chunk)
            {
                GUILayout.BeginVertical();

                GUILayout.Label(hashKey, MpsGui.SliderLabelStyle, GUILayout.ExpandWidth(false));

                var newValue = GUILayout.HorizontalSlider(
                    blendValue, 0f, 1f, MpsGui.SliderStyle, MpsGui.SliderThumbStyle, sliderWidth);

                if (!Mathf.Approximately(blendValue, newValue))
                    CurrentShapeKeyController[hashKey] = newValue;

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation() =>
        paneHeader.Label = Translation.Get("propShapeKeyPane", "header");
}
