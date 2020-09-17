using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropsPane : BasePane
    {
        private readonly PropManager propManager;
        private string currentCategory;
        private string SelectedCategory => Constants.DoguCategories[this.doguCategoryDropdown.SelectedItemIndex];
        private readonly Dropdown doguCategoryDropdown;
        private readonly Dropdown doguDropdown;
        private readonly Button addDoguButton;
        private readonly Button nextDoguButton;
        private readonly Button prevDoguButton;
        private readonly Button nextDoguCategoryButton;
        private readonly Button prevDoguCategoryButton;
        private static bool handItemsReady = false;
        private bool itemSelectorEnabled = true;

        public PropsPane(PropManager propManager)
        {
            this.propManager = propManager;

            handItemsReady = Constants.HandItemsInitialized;
            if (!handItemsReady) Constants.MenuFilesChange += InitializeHandItems;

            doguCategoryDropdown = new Dropdown(Translation.GetArray("doguCategories", Constants.DoguCategories));
            doguCategoryDropdown.SelectionChange += (s, a) => ChangeDoguCategory(SelectedCategory);

            doguDropdown = new Dropdown(new[] { string.Empty });

            addDoguButton = new Button("+");
            addDoguButton.ControlEvent += (s, a) => SpawnObject();

            nextDoguButton = new Button(">");
            nextDoguButton.ControlEvent += (s, a) => doguDropdown.Step(1);

            prevDoguButton = new Button("<");
            prevDoguButton.ControlEvent += (s, a) => doguDropdown.Step(-1);

            nextDoguCategoryButton = new Button(">");
            nextDoguCategoryButton.ControlEvent += (s, a) => doguCategoryDropdown.Step(1);

            prevDoguCategoryButton = new Button("<");
            prevDoguCategoryButton.ControlEvent += (s, a) => doguCategoryDropdown.Step(-1);

            ChangeDoguCategory(SelectedCategory);
        }

        protected override void ReloadTranslation()
        {
            doguCategoryDropdown.SetDropdownItems(
                Translation.GetArray("doguCategories", Constants.DoguCategories)
            );

            string category = SelectedCategory;

            string[] translationArray;

            if (category == Constants.customDoguCategories[Constants.DoguCategory.HandItem] && !handItemsReady)
            {
                translationArray = new[] { Translation.Get("systemMessage", "initializing") };
            }
            else
            {
                translationArray = GetTranslations(category);
            }
            doguDropdown.SetDropdownItems(translationArray);
        }

        public override void Draw()
        {
            const float buttonHeight = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(buttonHeight),
                GUILayout.Height(buttonHeight)
            };

            const float dropdownButtonWidth = 120f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(buttonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            GUILayout.BeginHorizontal();
            prevDoguCategoryButton.Draw(arrowLayoutOptions);
            doguCategoryDropdown.Draw(dropdownLayoutOptions);
            nextDoguCategoryButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUI.enabled = itemSelectorEnabled;
            GUILayout.BeginHorizontal();
            doguDropdown.Draw(dropdownLayoutOptions);
            prevDoguButton.Draw(arrowLayoutOptions);
            nextDoguButton.Draw(arrowLayoutOptions);
            addDoguButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void InitializeHandItems(object sender, MenuFilesEventArgs args)
        {
            if (args.Type == MenuFilesEventArgs.EventType.HandItems)
            {
                handItemsReady = true;
                string selectedCategory = SelectedCategory;
                if (selectedCategory == Constants.customDoguCategories[Constants.DoguCategory.HandItem])
                {
                    ChangeDoguCategory(selectedCategory, true);
                }
            }
        }

        private void ChangeDoguCategory(string category, bool force = false)
        {
            if (category != currentCategory || force)
            {
                currentCategory = category;

                string[] translationArray;

                if (category == Constants.customDoguCategories[Constants.DoguCategory.HandItem] && !handItemsReady)
                {
                    translationArray = new[] { Translation.Get("systemMessage", "initializing") };
                    itemSelectorEnabled = false;
                }
                else
                {
                    translationArray = GetTranslations(category);
                    itemSelectorEnabled = true;
                }
                doguDropdown.SetDropdownItems(translationArray, 0);
            }
        }

        private string[] GetTranslations(string category)
        {
            IEnumerable<string> itemList = Constants.DoguDict[category];
            if (category == Constants.customDoguCategories[Constants.DoguCategory.HandItem])
            {
                itemList = itemList.Select(item =>
                {
                    string handItemAsOdogu = Utility.HandItemToOdogu(item);

                    if (Translation.Has("propNames", handItemAsOdogu)) return handItemAsOdogu;
                    else return item;
                });
            }

            string translationCategory = category == Constants.customDoguCategories[Constants.DoguCategory.BGSmall]
                ? "bgNames"
                : "propNames";

            return Translation.GetArray(translationCategory, itemList);
        }

        private void SpawnObject()
        {
            string assetName = Constants.DoguDict[SelectedCategory][doguDropdown.SelectedItemIndex];
            if (SelectedCategory == Constants.customDoguCategories[Constants.DoguCategory.BGSmall])
            {
                propManager.SpawnBG(assetName);
            }
            else
            {
                propManager.SpawnObject(assetName);
            }
        }
    }
}
