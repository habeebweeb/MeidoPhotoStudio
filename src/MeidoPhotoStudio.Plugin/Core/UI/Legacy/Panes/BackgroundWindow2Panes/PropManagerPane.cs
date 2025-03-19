using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropManagerPane : BasePane
{
    private readonly PropService propService;
    private readonly FavouritePropRepository favouritePropRepository;
    private readonly PropDragHandleService propDragHandleService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TransformClipboard transformClipboard;
    private readonly Dropdown<PropController> propDropdown;
    private readonly Dictionary<PropController, string> propNames = [];
    private readonly Toggle dragPointToggle;
    private readonly Toggle gizmoToggle;
    private readonly Toggle shadowCastingToggle;
    private readonly Toggle visibleToggle;
    private readonly Button deletePropButton;
    private readonly Button copyPropButton;
    private readonly Toggle.Group gizmoModeGroup;
    private readonly Dictionary<CustomGizmo.GizmoMode, Toggle> gizmoModeToggles;
    private readonly SubPaneHeader transformPaneHeader;
    private readonly TransformInputPane transformInputPane;
    private readonly Button focusButton;
    private readonly Button addToFavouritesButton;
    private readonly Button removeFromFavouritesButton;
    private readonly Toggle toggleAllDragHandles;
    private readonly Toggle toggleAllGizmos;
    private readonly Label gizmoSpaceLabel;
    private readonly Header toggleAllHandlesHeader;
    private readonly Label noPropsLabel;
    private readonly LazyStyle noPropsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(Label.Style)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private bool isFavouriteProp;

    public PropManagerPane(
        Translation translation,
        PropService propService,
        FavouritePropRepository favouritePropRepository,
        PropDragHandleService propDragHandleService,
        SelectionController<PropController> propSelectionController,
        TransformClipboard transformClipboard)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.favouritePropRepository = favouritePropRepository ?? throw new ArgumentNullException(nameof(favouritePropRepository));
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.transformClipboard = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        this.propService.AddedProp += OnAddedProp;
        this.propService.RemovedProp += OnRemovedProp;
        this.favouritePropRepository.AddedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.RemovedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.Refreshed += OnFavouritePropRepositoryRefreshed;
        this.propSelectionController.Selecting += OnSelectingProp;
        this.propSelectionController.Selected += OnSelectedProp;

        propDropdown = new(formatter: PropNameFormatter);
        propDropdown.SelectionChanged += OnPropDropdownSelectionChange;

        dragPointToggle = new(new LocalizableGUIContent(translation, "propManagerPane", "dragPointToggle"));
        dragPointToggle.ControlEvent += OnDragPointToggleChanged;

        gizmoToggle = new(new LocalizableGUIContent(translation, "propManagerPane", "gizmoToggle"));
        gizmoToggle.ControlEvent += OnGizmoToggleChanged;

        shadowCastingToggle = new(new LocalizableGUIContent(translation, "propManagerPane", "shadowCastingToggle"));
        shadowCastingToggle.ControlEvent += OnShadowCastingToggleChanged;

        visibleToggle = new(new LocalizableGUIContent(translation, "propManagerPane", "visibleToggle"), true);
        visibleToggle.ControlEvent += OnVisibleToggleChanged;

        copyPropButton = new(new LocalizableGUIContent(translation, "propManagerPane", "copyButton"));
        copyPropButton.ControlEvent += OnCopyButtonPressed;

        deletePropButton = new(new LocalizableGUIContent(translation, "propManagerPane", "deleteButton"));
        deletePropButton.ControlEvent += OnDeleteButtonPressed;

        var localSpaceToggle = new Toggle(new LocalizableGUIContent(translation, "propManagerPane", "gizmoSpaceLocal"));

        localSpaceToggle.ControlEvent += OnGizmoModeToggleChanged(CustomGizmo.GizmoMode.Local);

        var worldSpaceToggle = new Toggle(new LocalizableGUIContent(translation, "propManagerPane", "gizmoSpaceWorld"), true);

        worldSpaceToggle.ControlEvent += OnGizmoModeToggleChanged(CustomGizmo.GizmoMode.World);

        gizmoModeGroup = [localSpaceToggle, worldSpaceToggle];

        gizmoModeToggles = new()
        {
            [CustomGizmo.GizmoMode.Local] = localSpaceToggle,
            [CustomGizmo.GizmoMode.World] = worldSpaceToggle,
        };

        focusButton = new(new LocalizableGUIContent(translation, "propManagerPane", "focusPropButton"));
        focusButton.ControlEvent += OnFocusButtonPushed;

        toggleAllDragHandles = new(new LocalizableGUIContent(translation, "propManagerPane", "allDragHandleToggle"), true);
        toggleAllDragHandles.ControlEvent += OnToggleAllDragHandlesChanged;

        toggleAllGizmos = new(new LocalizableGUIContent(translation, "propManagerPane", "allGizmoToggle"), true);
        toggleAllGizmos.ControlEvent += OnToggleAllGizmosChanged;

        gizmoSpaceLabel = new(new LocalizableGUIContent(translation, "propManagerPane", "gizmoSpaceToggle"));

        addToFavouritesButton = new(new LocalizableGUIContent(translation, "propManagerPane", "addFavouriteButton"));
        addToFavouritesButton.ControlEvent += OnAddFavouritePropButtonPushed;

        removeFromFavouritesButton = new(new LocalizableGUIContent(translation, "propManagerPane", "removeFavouriteButton"));
        removeFromFavouritesButton.ControlEvent += OnRemoveFavouritePropButtonPushed;

        transformPaneHeader = new(new LocalizableGUIContent(translation, "propManagerPane", "preciseTransformToggle"), false);
        transformInputPane = new(translation, this.transformClipboard);
        Add(transformInputPane);

        toggleAllHandlesHeader = new(new LocalizableGUIContent(translation, "propManagerPane", "toggleAllHandlesHeader"));

        noPropsLabel = new(new LocalizableGUIContent(translation, "propManagerPane", "noProps"));

        LabelledDropdownItem PropNameFormatter(PropController prop, int index) =>
            new(propNames[prop]);

        EventHandler OnGizmoModeToggleChanged(CustomGizmo.GizmoMode mode) =>
            (sender, _) =>
            {
                if (sender is not Toggle { Value: true } || CurrentProp is not PropController prop)
                    return;

                propDragHandleService[prop].GizmoMode = mode;
            };
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    public override void Draw()
    {
        if (propService.Count is 0)
        {
            noPropsLabel.Draw(noPropsLabelStyle);

            return;
        }

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        var noExpandWidth = GUILayout.ExpandWidth(false);

        GUILayout.BeginHorizontal();
        dragPointToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();
        focusButton.Draw(noExpandWidth);
        copyPropButton.Draw(noExpandWidth);
        deletePropButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        gizmoToggle.Draw(noExpandWidth);
        GUILayout.FlexibleSpace();

        var guiEnabled = Parent.Enabled;

        GUI.enabled = guiEnabled && gizmoToggle.Value;

        gizmoSpaceLabel.Draw();

        foreach (var gizmoModeToggle in gizmoModeGroup)
            gizmoModeToggle.Draw();

        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        visibleToggle.Draw(noExpandWidth);

        GUI.enabled = guiEnabled && visibleToggle.Value;
        shadowCastingToggle.Draw(noExpandWidth);
        GUI.enabled = guiEnabled;

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        if (isFavouriteProp)
            removeFromFavouritesButton.Draw();
        else
            addToFavouritesButton.Draw();

        toggleAllHandlesHeader.Draw();

        GUILayout.BeginHorizontal();

        toggleAllDragHandles.Draw();
        toggleAllGizmos.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        transformPaneHeader.Draw();

        if (transformPaneHeader.Enabled)
            transformInputPane.Draw();
    }

    private void UpdateControls()
    {
        transformInputPane.Target = CurrentProp;

        if (CurrentProp is null)
            return;

        shadowCastingToggle.SetEnabledWithoutNotify(CurrentProp.ShadowCasting);
        visibleToggle.SetEnabledWithoutNotify(CurrentProp.Visible);

        var dragHandleController = propDragHandleService[CurrentProp];

        dragPointToggle.SetEnabledWithoutNotify(dragHandleController.CubeEnabled);
        gizmoToggle.SetEnabledWithoutNotify(dragHandleController.GizmoEnabled);
        gizmoModeToggles[dragHandleController.GizmoMode].SetEnabledWithoutNotify(true);
    }

    private void OnToggleAllDragHandlesChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.CubeEnabled = toggleAllDragHandles.Value;
    }

    private void OnToggleAllGizmosChanged(object sender, EventArgs e)
    {
        foreach (var controller in propDragHandleService)
            controller.GizmoEnabled = toggleAllGizmos.Value;
    }

    private void OnAddedProp(object sender, PropServiceEventArgs e)
    {
        propNames[e.PropController] = UniquePropName([.. propNames.Values], e.PropController.PropModel);
        propDropdown.SetItems(propService, propService.Count - 1);

        static string UniquePropName(HashSet<string> currentNames, IPropModel propModel)
        {
            var propName = PropName(propModel);
            var newPropName = propName;
            var index = 1;

            while (currentNames.Contains(newPropName))
            {
                index++;
                newPropName = $"{propName} ({index})";
            }

            return newPropName;
        }

        static string PropName(IPropModel propModel) =>
            propModel.Name;
    }

    private void OnRemovedProp(object sender, PropServiceEventArgs e)
    {
        if (propService.Count is 0)
        {
            propDropdown.Clear();
            propNames.Clear();

            return;
        }

        var propIndex = propDropdown.SelectedItemIndex >= propService.Count
            ? propService.Count - 1
            : propDropdown.SelectedItemIndex;

        propNames.Remove(e.PropController);
        propDropdown.SetItems(propService, propIndex);
    }

    private void OnSelectingProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.PropertyChanged -= OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged -= OnDragHandlePropertyChanged;
    }

    private void OnSelectedProp(object sender, SelectionEventArgs<PropController> e)
    {
        if (e.Selected is not PropController prop)
            return;

        prop.PropertyChanged += OnPropPropertyChanged;

        var dragHandleController = propDragHandleService[prop];

        dragHandleController.PropertyChanged += OnDragHandlePropertyChanged;

        propDropdown.SetSelectedIndexWithoutNotify(e.Index);

        isFavouriteProp = favouritePropRepository.ContainsProp(prop.PropModel);

        UpdateControls();
    }

    private void OnPropPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var prop = (PropController)sender;

        if (e.PropertyName is nameof(PropController.ShadowCasting))
            shadowCastingToggle.SetEnabledWithoutNotify(prop.ShadowCasting);
        else if (e.PropertyName is nameof(PropController.Visible))
            visibleToggle.SetEnabledWithoutNotify(prop.Visible);
    }

    private void OnDragHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var controller = (PropDragHandleController)sender;

        if (e.PropertyName is nameof(PropDragHandleController.CubeEnabled))
            dragPointToggle.SetEnabledWithoutNotify(controller.CubeEnabled);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoMode))
            gizmoModeToggles[controller.GizmoMode].SetEnabledWithoutNotify(true);
        else if (e.PropertyName is nameof(PropDragHandleController.GizmoEnabled))
            gizmoToggle.SetEnabledWithoutNotify(controller.GizmoEnabled);
    }

    private void OnPropDropdownSelectionChange(object sender, EventArgs e)
    {
        if (propService.Count is 0)
            return;

        propSelectionController.Select(propDropdown.SelectedItem);
    }

    private void OnDragPointToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.CubeEnabled = dragPointToggle.Value;
    }

    private void OnGizmoToggleChanged(object sender, EventArgs e)
    {
        var controller = propDragHandleService[CurrentProp];

        controller.GizmoEnabled = gizmoToggle.Value;
    }

    private void OnShadowCastingToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.ShadowCasting = shadowCastingToggle.Value;
    }

    private void OnVisibleToggleChanged(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Visible = visibleToggle.Value;
    }

    private void OnCopyButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Clone(propService.IndexOf(CurrentProp));
    }

    private void OnDeleteButtonPressed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        propService.Remove(propService.IndexOf(CurrentProp));
    }

    private void OnFocusButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        CurrentProp.Focus();
    }

    private void OnFavouritePropAddedOrRemoved(object sender, FavouritePropRepositoryEventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (e.FavouriteProp.PropModel != CurrentProp.PropModel)
            return;

        isFavouriteProp = favouritePropRepository.ContainsProp(e.FavouriteProp.PropModel);
    }

    private void OnFavouritePropRepositoryRefreshed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        isFavouriteProp = favouritePropRepository.ContainsProp(CurrentProp.PropModel);
    }

    private void OnAddFavouritePropButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (favouritePropRepository.ContainsProp(CurrentProp.PropModel))
            return;

        favouritePropRepository.Add(CurrentProp.PropModel);
    }

    private void OnRemoveFavouritePropButtonPushed(object sender, EventArgs e)
    {
        if (CurrentProp is null)
            return;

        if (!favouritePropRepository.ContainsProp(CurrentProp.PropModel))
            return;

        favouritePropRepository.Remove(CurrentProp.PropModel);
    }
}
