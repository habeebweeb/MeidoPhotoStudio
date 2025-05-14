using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class EditModeMaidService : IActivateable
{
    private static readonly Dictionary<Type, MonoBehaviour> UIControllerCache = [];
    private static readonly Dictionary<GameObject, (ButtonEdit ButtonEdit, UIButton UIButton)> UIPartsButtonCache = [];

    private static GameObject uiRoot;

    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly CharacterRepository characterRepository;
    private readonly CharacterService characterService;

    public EditModeMaidService(
        CustomMaidSceneService customMaidSceneService,
        CharacterRepository characterRepository,
        CharacterService characterService)
    {
        this.customMaidSceneService = customMaidSceneService
            ?? throw new ArgumentNullException(nameof(customMaidSceneService));

        this.characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));

        this.characterService.PreCalledCharacters += OnCharactersPreCalled;
    }

    public event EventHandler<EditModeMaidServiceEventArgs> ChangingEditMaid;

    public event EventHandler<EditModeMaidServiceEventArgs> ChangedEditMaid;

    public CharacterModel EditingCharacter { get; private set; }

    public CharacterModel OriginalEditingCharacter { get; private set; }

    private static GameObject UIRoot =>
        uiRoot ? uiRoot : uiRoot = GameObject.Find("UI Root");

    private bool EditMode =>
        customMaidSceneService.EditScene;

    public void SetEditingCharacter(CharacterModel character)
    {
        if (!EditMode)
            return;

        _ = character ?? throw new ArgumentNullException(nameof(character));

        ChangingEditMaid?.Invoke(this, new(character.Maid, character));

        EditingCharacter = character;

        SetEditUIEnabled(true);

        UpdateCharacterMgr(character.Maid);

        UpdateEditModeUITarget(character.Maid);

        ChangedEditMaid?.Invoke(this, new(character.Maid, character));
    }

    void IActivateable.Activate()
    {
        if (!EditMode)
            return;

        OriginalEditingCharacter = characterRepository.GetByID(SceneEdit.Instance.m_maid.ID());
        EditingCharacter = OriginalEditingCharacter;

        AddOkButtonIntegration();

        if (OriginalEditingCharacter is not null)
            characterService.Call([OriginalEditingCharacter]);

        void AddOkButtonIntegration()
        {
            if (!TryGetUIControl<EditOkCancel>(out var button))
                return;

            button.m_dgOnClickOk -= RestoreOriginalEditingCharacter;
            button.m_dgOnClickOk = RestoreOriginalEditingCharacter + button.m_dgOnClickOk;
        }
    }

    void IActivateable.Deactivate()
    {
        if (!EditMode)
            return;

        UICamera.InputEnable = true;

        RestoreOriginalEditingCharacter();
        RemoveOkButtonIntegration();

        void RemoveOkButtonIntegration()
        {
            if (!TryGetUIControl<EditOkCancel>(out var button))
                return;

            button.m_dgOnClickOk -= RestoreOriginalEditingCharacter;
        }
    }

    // NOTE: Changing the edit maid's position to 0 is required to get parts of the edit mode functionality to work,
    // most notably the parts colouring feature.
    private static void UpdateCharacterMgr(Maid targetMaid)
    {
        var slot0Maid = GameMain.Instance.CharacterMgr.m_gcActiveMaid[0];
        var activeMaids = GameMain.Instance.CharacterMgr.m_gcActiveMaid;
        var activeGameObjects = GameMain.Instance.CharacterMgr.m_objActiveMaid;

        if (!slot0Maid || slot0Maid.Equals(targetMaid))
        {
            targetMaid.ActiveSlotNo = 0;
            activeMaids[0] = targetMaid;
            activeGameObjects[0] = targetMaid.gameObject;

            return;
        }

        (slot0Maid.ActiveSlotNo, targetMaid.ActiveSlotNo) = (targetMaid.ActiveSlotNo, slot0Maid.ActiveSlotNo);

        var targetMaidIndex = activeMaids.FindIndex(targetMaid.ValueEquals);

        if (targetMaidIndex is -1)
        {
            activeMaids[0] = targetMaid;
            activeGameObjects[0] = targetMaid.gameObject;
        }
        else
        {
            (activeMaids[0], activeMaids[targetMaidIndex]) = (targetMaid, slot0Maid);
            (activeGameObjects[0], activeGameObjects[targetMaidIndex]) = (targetMaid.gameObject, slot0Maid.gameObject);
        }
    }

    private static void UpdateEditModeUITarget(Maid maid)
    {
        var sceneEdit = SceneEdit.Instance;

        if (!sceneEdit)
            return;

        sceneEdit.m_maid = maid;

        if (TryGetUIControl<PresetCtrl>(out var presetCtrl))
            presetCtrl.m_maid = maid;

        if (TryGetUIControl<PresetButtonCtrl>(out var presetButtonCtrl))
            presetButtonCtrl.m_maid = maid;

        if (TryGetUIControl<ProfileCtrl>(out var profileCtrl))
            profileCtrl.m_maidStatus = maid.status;

        if (TryGetUIControl<SceneEditWindow.CustomPartsWindow>(out var sceneEditWindow))
        {
            sceneEditWindow.maid = maid;

            // Stopping maid animation and head movement when customizing parts placement
            sceneEditWindow.animation = maid.GetAnimation();
        }

        if (TryGetUIControl<SceneEditWindow.VoiceIconWindow>(out var voiceIconWindow))
            voiceIconWindow.pitchInput.value = maid.VoicePitch;

        sceneEdit.customViewWindow.UpdateAllItem();

        UpdatePartTypeAvailability(maid);

        if (sceneEdit.m_Panel_SliderItem.goMain.activeSelf)
            sceneEdit.UpdateSliders();
        else if (sceneEdit.m_Panel_MenuItem.goMain.activeSelf)
            UpdateSelectedMenuItem(maid);

        static void UpdatePartTypeAvailability(Maid maid)
        {
            var sceneEdit = SceneEdit.Instance;

            if (!sceneEdit)
                return;

            var isNewFace = maid != null && maid.body0 != null && CMT.SearchObjName(maid.body0.m_trBones, "Ear_L", boSMPass: false) != null;
            var isFBFace = maid != null && maid.body0 != null && maid.body0.GetSlot(1).PartsVersion >= 120;

            foreach (var item in sceneEdit.m_listCategory)
            {
                foreach (var partsType in item.m_listPartsType)
                {
                    if (partsType.m_mpn == MPN.accmimi)
                    {
                        partsType.m_isEnabled = maid.GetProp(MPN.EarNone).value == 0;

                        if (!partsType.m_isEnabled && !isNewFace)
                            partsType.m_isEnabled = true;
                    }
                    else if (partsType.m_eType == SceneEditInfo.CCateNameType.EType.Item)
                    {
                        if (SceneEditInfo.m_dicPartsTypePair[partsType.m_mpn].m_requestNewFace)
                            partsType.m_isEnabled = isNewFace;
                        else if (SceneEditInfo.m_dicPartsTypePair[partsType.m_mpn].m_requestFBFace)
                            partsType.m_isEnabled = isFBFace;
                    }
                    else
                    {
                        var requestNewFace = partsType.m_listMenu.Count > 0;
                        var requestFBFace = true;

                        foreach (var menuItem in partsType.m_listMenu)
                        {
                            if (!menuItem.m_requestNewFace)
                                requestNewFace = false;

                            if (!menuItem.m_requestFBFace)
                                requestFBFace = false;
                        }

                        partsType.m_isEnabled =
                            requestNewFace ? isNewFace :
                            requestFBFace ? isFBFace :
                            true;
                    }
                }
            }

            foreach (var partButton in sceneEdit.m_listBtnPartsType)
            {
                if (!partButton)
                    continue;

                if (!TryGetPartsButton(partButton, out var partsButton))
                    continue;

                var (buttonEdit, uiButton) = partsButton;

                uiButton.isEnabled = buttonEdit.m_PartsType.m_isEnabled;

                if (uiButton.isEnabled && (uiButton.onClick is null || uiButton.onClick.Count is 0))
                    EventDelegate.Add(uiButton.onClick, sceneEdit.ClickCallback);

                if (!buttonEdit.m_PartsType.m_isEnabled && sceneEdit.NowMPN == buttonEdit.m_PartsType.m_mpn)
                {
                    uiButton.defaultColor = buttonEdit.m_colBtnDefault;

                    var frame = buttonEdit.m_goFrame.GetComponent<UISprite>();

                    frame.enabled = false;

                    sceneEdit.m_Panel_PartsType.VisibleArrow(false);

                    DeselectCurrentPartType();
                }
            }
        }

        static void UpdateSelectedMenuItem(Maid maid)
        {
            var mpn = SceneEdit.Instance.NowMPN;

            if (mpn == MPN.null_mpn)
                return;

            var currentPartType = SceneEdit.Instance.CategoryList
                .SelectMany(category => category.m_listPartsType)
                .FirstOrDefault(partType => partType.m_mpn == mpn);

            if (currentPartType == null)
                return;

            SceneEdit.Instance.UpdateSelectedMenuItem(currentPartType);
        }
    }

    private static void SetEditUIEnabled(bool enabled)
    {
        UICamera.InputEnable = true;

        if (!UIRoot)
            return;

        var presetButtonPanel = UTY.GetChildObjectNoError(UIRoot, "PresetButtonPanel");

        if (!presetButtonPanel)
            return;

        if (TryGetUIControl<WindowPartsWindowVisible>(presetButtonPanel, "WindowPose", out var windowPoseButton))
            windowPoseButton.enabled = enabled;

        if (TryGetUIControl<WindowPartsWindowVisible>(presetButtonPanel, "WindowUndress", out var windowClothesButton))
            windowClothesButton.enabled = enabled;

        if (TryGetUIControl<WindowPartsWindowVisible>(presetButtonPanel, "WindowPresetSave", out var windowPresetButton))
            windowPresetButton.enabled = enabled;

        if (TryGetUIControl<WindowPartsWindowVisible>(presetButtonPanel, "WindowVoice", out var windowVoiceButton))
            windowVoiceButton.enabled = enabled;

        if (TryGetUIControl<WindowPartsWindowVisible>(presetButtonPanel, "WindowCustomView", out var windowCustomViewButton))
            windowCustomViewButton.enabled = enabled;

        if (TryGetUIControl<UIButton>(presetButtonPanel, "GroupSwitch", out var windowItemGroupButton))
            windowItemGroupButton.isEnabled = enabled;

        if (TryGetUIControl<UIButton>(presetButtonPanel, "TouchJumpSwitch", out var windowTouchJumpButton))
            windowTouchJumpButton.isEnabled = enabled;

        if (TryGetUIControl<UIButton>(presetButtonPanel, "HowToName", out var windowHowToNameButton))
            windowHowToNameButton.isEnabled = enabled;

        var viewResetPanel = UTY.GetChildObjectNoError(UIRoot, "ViewReset");

        if (TryGetUIControl<UIButton>(viewResetPanel, "EyeToCam", out var windowEyeToCamButton))
            windowEyeToCamButton.isEnabled = enabled;

        if (SceneEdit.Instance && SceneEdit.Instance.m_Panel_Category?.gcUIGrid)
            foreach (var buttonEdit in SceneEdit.Instance.m_Panel_Category.gcUIGrid.GetComponentsInChildren<ButtonEdit>())
                buttonEdit.GetComponent<UIButton>().isEnabled = enabled;

        static bool TryGetUIControl<T>(GameObject root, string path, out T uiControl)
            where T : MonoBehaviour
        {
            uiControl = null;

            var uiElement = UTY.GetChildObjectNoError(root, path);

            if (!uiElement)
                return false;

            uiControl = uiElement.GetComponent<T>();

            return uiControl;
        }
    }

    private static void CloseAllEditPanels()
    {
        var sceneEdit = SceneEdit.Instance;

        if (!sceneEdit)
            return;

        if (!UIRoot)
            return;

        DeselectCurrentPartType(closePanel: true);

        if (TryGetUIControl<SceneEditWindow.UndressWindow>(out var undressWindow))
            undressWindow.visible = false;

        if (TryGetUIControl<SceneEditWindow.VoiceIconWindow>(out var voiceIconWindow))
            voiceIconWindow.visible = false;

        if (TryGetUIControl<SceneEditWindow.PresetSaveWindow>(out var presetSaveWindow))
            presetSaveWindow.visible = false;

        sceneEdit.customViewWindow.visible = false;
        sceneEdit.pauseIconWindow.visible = false;
        sceneEdit.m_FFNameDlg.Close();

        sceneEdit.CategoryUnSelect();

        sceneEdit.SetCameraOffset(SceneEdit.CAM_OFFS.CENTER);
    }

    private static void DeselectCurrentPartType(bool closePanel = false)
    {
        var sceneEdit = SceneEdit.Instance;

        if (!sceneEdit)
            return;

        if (!UIRoot)
            return;

        sceneEdit.m_nNowMPN = MPN.null_mpn;
        sceneEdit.m_Panel_MenuItem.SetActive(false);
        sceneEdit.m_Panel_SetItem.SetActive(false);
        sceneEdit.m_Panel_SliderItem.SetActive(false);
        sceneEdit.m_Panel_ColorSet.SetActive(false);
        sceneEdit.m_Panel_GroupSet.SetActive(false);

        sceneEdit.colorPaletteMgr.Close();
        sceneEdit.customPartsWindow.visible = false;
        sceneEdit.customPartsWindowVisibleButton.visible = false;
        sceneEdit.highlightSelector.visible = false;

        if (TryGetUIControl<BodyStatusMgr>(out var bodyStatusMgr))
        {
            bodyStatusMgr.CloseBodyPanel();
            bodyStatusMgr.CloseMotionPanel();
        }

        if (TryGetUIControl<PresetMgr>(out var presetMgr))
            presetMgr.ClosePresetPanel();

        if (TryGetUIControl<RandomPresetMgr>(out var randomPresetMgr))
            randomPresetMgr.CloseRandomPresetPanel();

        if (TryGetUIControl<ProfileMgr>(out var profileMgr))
        {
            profileMgr.CloseProfilePanel();
            profileMgr.CloseSubWindowIfOpen();
        }

        if (TryGetUIControl<CostumePartsEnabledMgr>(out var costumePartsEnabledMgr))
            costumePartsEnabledMgr.CloseRandomPresetPanel();

        if (TryGetUIControl<HairLongWindow>(out var hairLongWindow))
            hairLongWindow.visible = false;

        sceneEdit.SetCameraOffset(SceneEdit.CAM_OFFS.CENTER);

        if (closePanel)
        {
            sceneEdit.m_Panel_PartsType.SetActive(false);
        }
        else
        {
            foreach (var partButton in sceneEdit.m_listBtnPartsType.Where(static part => part))
            {
                if (!TryGetPartsButton(partButton, out var partsButton))
                    continue;

                if (partsButton.ButtonEdit.m_PartsType.m_mpn != sceneEdit.m_nNowMPN)
                    continue;

                var (buttonEdit, uiButton) = partsButton;

                uiButton.defaultColor = buttonEdit.m_colBtnDefault;

                var frame = buttonEdit.m_goFrame.GetComponent<UISprite>();

                frame.enabled = false;

                sceneEdit.m_Panel_PartsType.VisibleArrow(false);

                break;
            }
        }
    }

    private static bool TryGetUIControl<T>(out T uiControl)
        where T : MonoBehaviour
    {
        uiControl = null;

        if (!UIRoot)
            return false;

        if (UIControllerCache.TryGetValue(typeof(T), out var cachedControl) && cachedControl)
        {
            uiControl = (T)cachedControl;

            return true;
        }

        uiControl = UIRoot.GetComponentInChildren<T>(true);

        if (!uiControl)
            return false;

        UIControllerCache[typeof(T)] = uiControl;

        return true;
    }

    private static bool TryGetPartsButton(GameObject partsGameObject, out (ButtonEdit ButtonEdit, UIButton UIButton) buttons)
    {
        if (UIPartsButtonCache.TryGetValue(partsGameObject, out buttons) && buttons.ButtonEdit && buttons.UIButton)
            return true;

        var buttonEdit = partsGameObject.GetComponentInChildren<ButtonEdit>(includeInactive: true);
        var uiButton = partsGameObject.GetComponentInChildren<UIButton>(includeInactive: true);

        if (!buttonEdit || !uiButton)
            return false;

        buttons = UIPartsButtonCache[partsGameObject] = (buttonEdit, uiButton);

        return true;
    }

    private void OnCharactersPreCalled(object sender, CharacterServiceEventArgs e)
    {
        var editingCharacterIndex = -1;
        var originalEditingMaidIndex = -1;

        for (var i = 0; i < e.LoadedCharacters.Length; i++)
        {
            var character = e.LoadedCharacters[i];

            if (character.CharacterModel.Equals(EditingCharacter))
                editingCharacterIndex = i;

            if (character.CharacterModel.Equals(OriginalEditingCharacter))
                originalEditingMaidIndex = i;
        }

        if (editingCharacterIndex >= 0)
        {
            UpdateCharacterMgr(e.LoadedCharacters[editingCharacterIndex].Maid);
        }
        else if (originalEditingMaidIndex >= 0)
        {
            SetEditingCharacter(e.LoadedCharacters[originalEditingMaidIndex].CharacterModel);
        }
        else if (e.LoadedCharacters.Length > 0)
        {
            SetEditingCharacter(e.LoadedCharacters[0].CharacterModel);
        }
        else
        {
            UnsetEditMaid();
        }

        void UnsetEditMaid()
        {
            ChangingEditMaid?.Invoke(this, new(null, null));

            EditingCharacter = null;

            CloseAllEditPanels();
            SetEditUIEnabled(false);

            ChangedEditMaid?.Invoke(this, new(null, null));
        }
    }

    private void RestoreOriginalEditingCharacter()
    {
        if (OriginalEditingCharacter is null)
            return;

        try
        {
            SetEditingCharacter(OriginalEditingCharacter);
            OriginalEditingCharacter.Maid.Visible = true;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogDebug($"Unable to restore original editing maid\n{e}");
        }
    }
}
