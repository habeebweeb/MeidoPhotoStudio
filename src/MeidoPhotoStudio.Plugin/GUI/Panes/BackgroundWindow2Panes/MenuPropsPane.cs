using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MenuPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly MenuPropsConfiguration menuPropsConfiguration;
    private readonly IconCache iconCache;
    private readonly Dropdown propCategoryDropdown;
    private readonly Toggle modFilterToggle;
    private readonly Toggle baseFilterToggle;

    private MPN[] categories;
    private Vector2 scrollPosition;
    private IEnumerable<MenuFilePropModel> currentPropList;
    private bool menuDatabaseBusy = false;
    private string initializingMessage;

    public MenuPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository,
        MenuPropsConfiguration menuPropsConfiguration,
        IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.menuPropsConfiguration = menuPropsConfiguration;
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        propCategoryDropdown = new(new[] { ":)" });
        propCategoryDropdown.SelectionChange += OnPropCategoryDropdownChanged;

        modFilterToggle = new(Translation.Get("background2Window", "modsToggle"));
        modFilterToggle.ControlEvent += OnModFilterChanged;

        baseFilterToggle = new(Translation.Get("background2Window", "baseToggle"));
        baseFilterToggle.ControlEvent += OnBaseFilterChanged;

        initializingMessage = Translation.Get("systemMessage", "initializing");

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        void OnMenuDatabaseReady(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;

            Initialize();

            menuPropRepository.InitializedProps -= OnMenuDatabaseReady;
        }

        void Initialize()
        {
            categories = new[] { MPN.null_mpn }.Concat(
                menuPropRepository.CategoryMpn
                    .Where(mpn => mpn is not MPN.handitem)
                    .OrderBy(mpn => mpn))
                .ToArray();

            propCategoryDropdown.SetDropdownItems(categories
                .Select(mpn => Translation.Get("clothing", mpn.ToString()))
                .ToArray());
        }
    }

    private enum FilterType
    {
        None,
        Mod,
        Base,
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            GUILayout.Label(initializingMessage);

            return;
        }

        DrawDropdown(propCategoryDropdown);

        MpsGui.BlackLine();

        if (!menuPropsConfiguration.ModMenuPropsOnly)
        {
            DrawFilterToggles();

            MpsGui.BlackLine();
        }

        DrawPropList();

        static void DrawDropdown(Dropdown dropdown)
        {
            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            const float dropdownButtonWidth = 185f;

            var dropdownLayoutOptions = new[]
            {
                GUILayout.Width(dropdownButtonWidth),
            };

            GUILayout.BeginHorizontal();

            dropdown.Draw(dropdownLayoutOptions);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.Step(1);

            GUILayout.EndHorizontal();
        }

        void DrawPropList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            const float buttonSize = 55f;

            var propList = currentPropList;

            if (!menuPropsConfiguration.ModMenuPropsOnly)
            {
                if (modFilterToggle.Value)
                    propList = currentPropList.Where(prop => !prop.GameMenu);
                else if (baseFilterToggle.Value)
                    propList = currentPropList.Where(prop => prop.GameMenu);
            }

            var buttonStyle = new GUIStyle(GUI.skin.button) { margin = new(0, 0, 0, 0), padding = new(0, 0, 0, 0) };
            var buttonLayoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(buttonSize), GUILayout.Height(buttonSize),
            };

            foreach (var propChunk in propList.Chunk(4))
            {
                GUILayout.BeginHorizontal();

                foreach (var prop in propChunk)
                {
                    var image = iconCache.GetMenuIcon(prop);
                    var clicked = image
                        ? GUILayout.Button(image, buttonStyle, buttonLayoutOptions)
                        : GUILayout.Button(prop.Name, buttonStyle, buttonLayoutOptions);

                    if (clicked)
                        propService.Add(prop);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        void DrawFilterToggles()
        {
            GUILayout.BeginHorizontal();

            modFilterToggle.Draw();
            baseFilterToggle.Draw();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        if (menuPropRepository.Busy)
            return;

        propCategoryDropdown.SetDropdownItemsWithoutNotify(
            categories.Select(mpn => Translation.Get("clothing", mpn.ToString())).ToArray());

        modFilterToggle.Label = Translation.Get("background2Window", "modsToggle");
        baseFilterToggle.Label = Translation.Get("background2Window", "baseToggle");

        initializingMessage = Translation.Get("systemMessage", "initializing");
    }

    private void UpdateCurrentPropList()
    {
        if (menuDatabaseBusy)
            return;

        var currentCategory = categories[propCategoryDropdown.SelectedItemIndex];

        if (currentCategory is MPN.null_mpn)
        {
            currentPropList = Enumerable.Empty<MenuFilePropModel>();

            return;
        }

        scrollPosition = Vector2.zero;

        currentPropList = menuPropRepository[currentCategory];
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        UpdateCurrentPropList();

    private void ChangeFilter(FilterType filterType)
    {
        if (!modFilterToggle.Value || !baseFilterToggle.Value)
            return;

        modFilterToggle.SetEnabledWithoutNotify(filterType is FilterType.Mod);
        baseFilterToggle.SetEnabledWithoutNotify(filterType is FilterType.Base);
    }

    private void OnModFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(FilterType.Mod);

    private void OnBaseFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(FilterType.Base);
}
