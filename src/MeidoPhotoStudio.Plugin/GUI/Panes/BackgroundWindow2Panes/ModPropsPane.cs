using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    public class ModPropsPane : BasePane
    {
        private readonly PropManager propManager;
        private readonly Dropdown propCategoryDropdown;
        private readonly Toggle modFilterToggle;
        private readonly Toggle baseFilterToggle;
        private Vector2 propListScrollPos;
        private string SelectedCategory => MenuCategories[propCategoryDropdown.SelectedItemIndex];
        private List<ModItem> modPropList;
        private string currentCategory;
        private bool modItemsReady;
        private bool shouldDraw;
        private int categoryIndex;
        private bool modFilter;
        private bool baseFilter;
        private int currentListCount;
        private readonly bool isModsOnly = PropManager.ModItemsOnly;
        private enum FilterType
        {
            None, Mod, Base
        }

        public ModPropsPane(PropManager propManager)
        {
            this.propManager = propManager;

            modItemsReady = MenuFilesReady || PropManager.ModItemsOnly;

            string[] listItems = Translation.GetArray("clothing", MenuCategories);

            if (!modItemsReady)
            {
                listItems[0] = Translation.Get("systemMessage", "initializing");

                MenuFilesReadyChange += (s, a) =>
                {
                    modItemsReady = true;
                    propCategoryDropdown.SetDropdownItems(
                        Translation.GetArray("clothing", MenuCategories)
                    );
                };
            }

            propCategoryDropdown = new Dropdown(listItems);

            propCategoryDropdown.SelectionChange += (s, a) =>
            {
                if (!modItemsReady) return;
                ChangePropCategory();
            };

            if (!isModsOnly)
            {
                modFilterToggle = new Toggle(Translation.Get("background2Window", "modsToggle"));
                modFilterToggle.ControlEvent += (s, a) => ChangeFilter(FilterType.Mod);

                baseFilterToggle = new Toggle(Translation.Get("background2Window", "baseToggle"));
                baseFilterToggle.ControlEvent += (s, a) => ChangeFilter(FilterType.Base);
            }
        }

        protected override void ReloadTranslation()
        {
            string[] listItems = Translation.GetArray("clothing", MenuCategories);

            if (!modItemsReady) listItems[0] = Translation.Get("systemMessage", "initializing");

            propCategoryDropdown.SetDropdownItems(listItems);

            if (!isModsOnly)
            {
                modFilterToggle.Label = Translation.Get("background2Window", "modsToggle");
                baseFilterToggle.Label = Translation.Get("background2Window", "baseToggle");
            }
        }

        public float buttonSize = 54f;
        public override void Draw()
        {
            const float dropdownButtonHeight = 30f;
            float dropdownButtonWidth = isModsOnly ? 120f : 90f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();

            if (isModsOnly)
            {
                GUILayout.FlexibleSpace();
                propCategoryDropdown.Draw(dropdownLayoutOptions);
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUI.enabled = modItemsReady;
                propCategoryDropdown.Draw(dropdownLayoutOptions);

                GUI.enabled = shouldDraw;
                modFilterToggle.Draw();
                baseFilterToggle.Draw();
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            if (shouldDraw)
            {
                Rect windowRect = parent.WindowRect;
                float windowHeight = windowRect.height;
                float windowWidth = windowRect.width;

                // const float buttonSize = 50f;
                const float offsetTop = 80f;
                const int columns = 4;
                float buttonSize = (windowWidth / columns) - 10f;

                Rect positionRect = new Rect(
                    5f, offsetTop + dropdownButtonHeight, windowWidth - 10f, windowHeight - 145f
                );
                Rect viewRect = new Rect(
                    0f, 0f, buttonSize * columns, (buttonSize * Mathf.Ceil(currentListCount / (float)columns)) + 5
                );
                propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

                int modIndex = 0;
                foreach (ModItem modItem in modPropList)
                {
                    if ((modFilter && !modItem.IsMod) || (baseFilter && modItem.IsMod)) continue;

                    float x = modIndex % columns * buttonSize;
                    float y = modIndex / columns * buttonSize;
                    Rect iconRect = new Rect(x, y, buttonSize, buttonSize);
                    if (GUI.Button(iconRect, "")) propManager.AddModProp(modItem);
                    GUI.DrawTexture(iconRect, modItem.Icon);
                    modIndex++;
                }

                GUI.EndScrollView();
            }
        }

        private void ChangeFilter(FilterType filterType)
        {
            if (updating) return;

            if (modFilterToggle.Value && baseFilterToggle.Value)
            {
                updating = true;
                modFilterToggle.Value = filterType == FilterType.Mod;
                baseFilterToggle.Value = filterType == FilterType.Base;
                updating = false;
            }

            modFilter = modFilterToggle.Value;
            baseFilter = baseFilterToggle.Value;

            SetListCount();
        }

        private void ChangePropCategory()
        {
            string category = SelectedCategory;

            if (currentCategory == category) return;
            currentCategory = category;

            categoryIndex = propCategoryDropdown.SelectedItemIndex;

            shouldDraw = categoryIndex > 0;

            if (!shouldDraw) return;

            propListScrollPos = Vector2.zero;

            modPropList = Constants.GetModPropList(category);

            SetListCount();
        }

        private void SetListCount()
        {
            if (modFilter) currentListCount = modPropList.Count(mod => mod.IsMod);
            else if (baseFilter) currentListCount = modPropList.Count(mod => !mod.IsMod);
            else currentListCount = modPropList.Count;
        }
    }
}
