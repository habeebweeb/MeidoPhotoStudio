using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MenuFileUtility;
    internal class ModPropsPane : BasePane
    {
        private PropManager propManager;
        private Dropdown propCategoryDropdown;
        private Toggle modFilterToggle;
        private Toggle baseFilterToggle;
        private Vector2 propListScrollPos;
        private string SelectedCategory
        {
            get => MenuFileUtility.MenuCategories[this.propCategoryDropdown.SelectedItemIndex];
        }
        private List<ModItem> modPropList;
        private string currentCategory;
        private bool modItemsReady = false;
        private bool shouldDraw = false;
        private int categoryIndex = 0;
        private bool modFilter = false;
        private bool baseFilter = false;
        private int currentListCount;
        private bool isModsOnly = Configuration.ModItemsOnly;
        private enum FilterType
        {
            None, Mod, Base
        }

        public ModPropsPane(PropManager propManager)
        {
            this.propManager = propManager;

            this.modItemsReady = MenuFileUtility.MenuFilesReady || Configuration.ModItemsOnly;

            string[] listItems = Translation.GetArray("clothing", MenuFileUtility.MenuCategories);

            if (!this.modItemsReady)
            {
                listItems[0] = Translation.Get("systemMessage", "initializing");

                MenuFileUtility.MenuFilesReadyChange += (s, a) =>
                {
                    this.modItemsReady = true;
                    this.propCategoryDropdown.SetDropdownItems(
                        Translation.GetArray("clothing", MenuFileUtility.MenuCategories)
                    );
                };
            }

            this.propCategoryDropdown = new Dropdown(listItems);

            this.propCategoryDropdown.SelectionChange += (s, a) =>
            {
                if (!this.modItemsReady) return;
                ChangePropCategory();
            };

            this.modFilterToggle = new Toggle(Translation.Get("background2Window", "modsToggle"));
            this.modFilterToggle.ControlEvent += (s, a) => ChangeFilter(FilterType.Mod);


            this.baseFilterToggle = new Toggle(Translation.Get("background2Window", "baseToggle"));
            this.baseFilterToggle.ControlEvent += (s, a) => ChangeFilter(FilterType.Base);
        }

        protected override void ReloadTranslation()
        {
            string[] listItems = Translation.GetArray("clothing", MenuFileUtility.MenuCategories);

            if (!this.modItemsReady) listItems[0] = Translation.Get("systemMessage", "initializing");

            this.propCategoryDropdown.SetDropdownItems(listItems);


            this.modFilterToggle.Label = Translation.Get("background2Window", "modsToggle");
            this.baseFilterToggle.Label = Translation.Get("background2Window", "baseToggle");
        }

        public override void Draw()
        {
            float dropdownButtonHeight = 30f;
            float dropdownButtonWidth = isModsOnly ? 120f : 90f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();

            if (isModsOnly)
            {
                GUILayout.FlexibleSpace();
                this.propCategoryDropdown.Draw(dropdownLayoutOptions);
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUI.enabled = this.modItemsReady;
                this.propCategoryDropdown.Draw(dropdownLayoutOptions);

                GUI.enabled = this.shouldDraw;
                this.modFilterToggle.Draw();
                this.baseFilterToggle.Draw();
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            if (this.shouldDraw)
            {
                float windowHeight = Screen.height * 0.7f;

                int buttonSize = 50;
                int offsetLeft = 15;
                int offsetTop = 85;

                int columns = 4;

                Rect positionRect = new Rect(offsetLeft, offsetTop + dropdownButtonHeight, 220, windowHeight);
                Rect viewRect = new Rect(
                    0, 0, buttonSize * columns, buttonSize * Mathf.Ceil(currentListCount / (float)columns) + 5
                );
                propListScrollPos = GUI.BeginScrollView(positionRect, propListScrollPos, viewRect);

                int modIndex = 0;
                foreach (ModItem modItem in modPropList)
                {
                    if ((modFilter && !modItem.IsMod) || (baseFilter && modItem.IsMod)) continue;

                    float x = modIndex % columns * buttonSize;
                    float y = modIndex / columns * buttonSize;
                    Rect iconRect = new Rect(x, y, buttonSize, buttonSize);
                    if (GUI.Button(iconRect, "")) propManager.SpawnModItemProp(modItem);
                    GUI.DrawTexture(iconRect, modItem.Icon);
                    modIndex++;
                }

                GUI.EndScrollView();
                GUILayout.Space(windowHeight);
            }
        }

        private void ChangeFilter(FilterType filterType)
        {
            if (this.updating) return;

            if (modFilterToggle.Value && baseFilterToggle.Value)
            {
                this.updating = true;
                modFilterToggle.Value = filterType == FilterType.Mod;
                baseFilterToggle.Value = filterType == FilterType.Base;
                this.updating = false;
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
            if (modFilter || isModsOnly) currentListCount = modPropList.Count(mod => mod.IsMod);
            else if (baseFilter) currentListCount = modPropList.Count(mod => !mod.IsMod);
            else currentListCount = modPropList.Count;
        }
    }
}
