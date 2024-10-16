using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropShapeKeyPane : BasePane
{
    private readonly SelectionController<PropController> propSelectionController;
    private readonly Dictionary<string, EventHandler> sliderChangeEvents = [];
    private readonly PaneHeader paneHeader = new(Translation.Get("propShapeKeyPane", "header"), true);
    private readonly Dictionary<string, Slider> sliders = new(StringComparer.Ordinal);

    public PropShapeKeyPane(SelectionController<PropController> propSelectionController)
    {
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));

        this.propSelectionController.Selecting += OnSelectingProp;
        this.propSelectionController.Selected += OnSelectedProp;
    }

    private ShapeKeyController CurrentShapeKeyController =>
        propSelectionController.Current?.ShapeKeyController;

    public override void Draw()
    {
        var enabled = CurrentShapeKeyController != null;

        if (!enabled)
            return;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        var sliderWidth = GUILayout.MaxWidth(parent.WindowRect.width / 2 - 10f);
        var maxWidth = GUILayout.MaxWidth(parent.WindowRect.width - 10f);

        foreach (var chunk in CurrentShapeKeyController.Keys.Chunk(2))
        {
            GUILayout.BeginHorizontal(maxWidth);

            foreach (var hashKey in chunk)
                sliders[hashKey].Draw(sliderWidth);

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation() =>
        paneHeader.Label = Translation.Get("propShapeKeyPane", "header");

    private void OnSelectingProp(object sender, SelectionEventArgs<PropController> e)
    {
        foreach (var (hashKey, slider) in sliders)
        {
            if (!sliderChangeEvents.TryGetValue(hashKey, out var @event))
                continue;

            slider.ControlEvent -= @event;
        }

        sliderChangeEvents.Clear();

        if (e.Selected?.ShapeKeyController is not ShapeKeyController controller)
            return;

        controller.ShapeKeyChanged -= OnShapeKeyChanged;
    }

    private void OnSelectedProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected?.ShapeKeyController is not ShapeKeyController controller)
            return;

        controller.ShapeKeyChanged += OnShapeKeyChanged;

        foreach (var (hashKey, blendValue) in controller)
        {
            if (!sliders.TryGetValue(hashKey, out var slider))
                slider = sliders[hashKey] = new(hashKey, 0f, 1f);

            slider.SetValueWithoutNotify(blendValue);

            sliderChangeEvents[hashKey] = SliderChangedEventHandler(hashKey);

            slider.ControlEvent += sliderChangeEvents[hashKey];
        }

        EventHandler SliderChangedEventHandler(string key) =>
            (object sender, EventArgs e) =>
                controller[key] = ((Slider)sender).Value;
    }

    private void OnShapeKeyChanged(object sender, KeyedPropertyChangeEventArgs<string> e)
    {
        if (!sliders.TryGetValue(e.Key, out var slider))
            return;

        var controller = (ShapeKeyController)sender;

        slider.SetValueWithoutNotify(controller[e.Key]);
    }
}
