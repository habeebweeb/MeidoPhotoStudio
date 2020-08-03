using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class PropsPane : BasePane
    {
        private PropManager propManager;
        private string currentCategory;
        private string SelectedCategory => Constants.DoguCategories[this.doguCategoryDropdown.SelectedItemIndex];
        private Dropdown doguCategoryDropdown;
        private Dropdown doguDropdown;
        private Button addDoguButton;
        private Button nextDoguButton;
        private Button prevDoguButton;
        private Button nextDoguCategoryButton;
        private Button prevDoguCategoryButton;
        private string header;
        private static bool handItemsReady = false;
        private bool itemSelectorEnabled = true;

        public PropsPane(PropManager propManager)
        {
            this.header = Translation.Get("propsPane", "header");

            this.propManager = propManager;

            handItemsReady = Constants.HandItemsInitialized;
            if (!handItemsReady) Constants.MenuFilesChange += InitializeHandItems;

            this.doguCategoryDropdown = new Dropdown(Translation.GetArray("doguCategories", Constants.DoguCategories));
            this.doguCategoryDropdown.SelectionChange += (s, a) => ChangeDoguCategory(SelectedCategory);

            this.doguDropdown = new Dropdown(new[] { string.Empty });

            this.addDoguButton = new Button("+");
            this.addDoguButton.ControlEvent += (s, a) => SpawnObject();

            this.nextDoguButton = new Button(">");
            this.nextDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(1);

            this.prevDoguButton = new Button("<");
            this.prevDoguButton.ControlEvent += (s, a) => this.doguDropdown.Step(-1);

            this.nextDoguCategoryButton = new Button(">");
            this.nextDoguCategoryButton.ControlEvent += (s, a) => this.doguCategoryDropdown.Step(1);

            this.prevDoguCategoryButton = new Button("<");
            this.prevDoguCategoryButton.ControlEvent += (s, a) => this.doguCategoryDropdown.Step(-1);

            ChangeDoguCategory(SelectedCategory);
        }

        protected override void ReloadTranslation()
        {
            this.header = Translation.Get("propsPane", "header");

            this.doguCategoryDropdown.SetDropdownItems(
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
            float arrowButtonSize = 30;
            GUILayoutOption[] arrowLayoutOptions = {
                GUILayout.Width(arrowButtonSize),
                GUILayout.Height(arrowButtonSize)
            };

            float dropdownButtonHeight = arrowButtonSize;
            float dropdownButtonWidth = 120f;
            GUILayoutOption[] dropdownLayoutOptions = new GUILayoutOption[] {
                GUILayout.Height(dropdownButtonHeight),
                GUILayout.Width(dropdownButtonWidth)
            };

            // MiscGUI.Header(this.header);
            // MiscGUI.WhiteLine();

            GUILayout.BeginHorizontal();
            this.prevDoguCategoryButton.Draw(arrowLayoutOptions);
            this.doguCategoryDropdown.Draw(dropdownLayoutOptions);
            this.nextDoguCategoryButton.Draw(arrowLayoutOptions);
            GUILayout.EndHorizontal();

            GUI.enabled = itemSelectorEnabled;
            GUILayout.BeginHorizontal();
            this.doguDropdown.Draw(dropdownLayoutOptions);
            this.prevDoguButton.Draw(arrowLayoutOptions);
            this.nextDoguButton.Draw(arrowLayoutOptions);
            this.addDoguButton.Draw(arrowLayoutOptions);
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
            string assetName = Constants.DoguDict[SelectedCategory][this.doguDropdown.SelectedItemIndex];
            if (SelectedCategory == Constants.customDoguCategories[Constants.DoguCategory.BGSmall])
            {
                assetName = "BG_" + assetName;
            }
            this.propManager.SpawnObject(assetName);
        }
    }
}
