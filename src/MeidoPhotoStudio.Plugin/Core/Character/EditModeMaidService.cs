using MeidoPhotoStudio.Database.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class EditModeMaidService
{
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly CharacterRepository characterRepository;

    public EditModeMaidService(
        CustomMaidSceneService customMaidSceneService, CharacterRepository characterRepository)
    {
        this.customMaidSceneService = customMaidSceneService
            ?? throw new ArgumentNullException(nameof(customMaidSceneService));

        this.characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));

        if (!customMaidSceneService.EditScene)
            return;

        UpdateOriginalEditingMaid();
        IntegrateWithOkButton();
    }

    public CharacterModel EditingCharacter { get; private set; }

    public CharacterModel OriginalEditingCharacter { get; private set; }

    private bool EditMode =>
        customMaidSceneService.EditScene;

    public void SetEditingCharacter(CharacterModel character)
    {
        if (!EditMode)
            return;

        _ = character ?? throw new ArgumentNullException(nameof(character));

        EditingCharacter = character;

        SetEditingMaid(character.Maid);
    }

    public void RestoreOriginalEditingMaid()
    {
        if (!EditMode)
            return;

        if (OriginalEditingCharacter.Maid == SceneEdit.Instance.m_maid)
            return;

        try
        {
            Utility.LogDebug($"Setting editing maid back to '{OriginalEditingCharacter.FullName()}'");

            SetEditingMaid(OriginalEditingCharacter.Maid);
        }
        catch (Exception e)
        {
            Utility.LogDebug($"Unable to restore original editing maid\n{e}");
        }
    }

    internal void Activate()
    {
        if (!EditMode)
            return;

        UpdateOriginalEditingMaid();
        IntegrateWithOkButton();
    }

    internal void Deactivate()
    {
        if (!EditMode)
            return;

        RestoreOriginalEditingMaid();
        RemoveOkButtonIntegration();
    }

    private void SetEditingMaid(Maid maid)
    {
        SceneEdit.Instance.m_maid = maid;

        UpdateCharacterMgr(maid);

        UpdateEditModeUI(maid);

        // TODO: WARN: Plugins that rely on CharacterMgr for the order that maids are called in by MPS will be out of
        // order. Not sure if this will be a problem but I can't figure out a way around this.
        // NOTE: Changing the edit maid's position to 0 is required to get parts of the edit mode functionality to work,
        // most notably the parts colouring feature.
        void UpdateCharacterMgr(Maid maid)
        {
            var activeMaids = GameMain.Instance.CharacterMgr.m_gcActiveMaid;
            var newEditMaidIndex = Array.IndexOf(activeMaids, maid);

            maid.ActiveSlotNo = 0;
            OriginalEditingCharacter.Maid.ActiveSlotNo = newEditMaidIndex;

            activeMaids[0] = maid;
            activeMaids[newEditMaidIndex] = OriginalEditingCharacter.Maid;
        }

        static void UpdateEditModeUI(Maid maid)
        {
            var uiRoot = GameObject.Find("UI Root");

            ApplyPresetApplicationControl(maid, uiRoot);

            ApplyPresetSavingControl(maid, uiRoot);

            ApplyMaidProfileControl(maid, uiRoot);

            ApplyAccessoryPlacementControl(maid, uiRoot);

            static void ApplyPresetApplicationControl(Maid maid, GameObject uiRoot)
            {
                if (!TryGetUIControl<PresetCtrl>(uiRoot, "PresetPanel", out var presetCtrl))
                {
                    Utility.LogDebug("Could not get 'PresetPanel'");

                    return;
                }

                presetCtrl.m_maid = maid;
            }

            static void ApplyPresetSavingControl(Maid maid, GameObject uiRoot)
            {
                if (!TryGetUIControl<PresetButtonCtrl>(uiRoot, "PresetButtonPanel", out var presetButtonCtrl))
                {
                    Utility.LogDebug("Could not get 'PresetButtonPanel'");

                    return;
                }

                presetButtonCtrl.m_maid = maid;
            }

            static void ApplyMaidProfileControl(Maid maid, GameObject uiRoot)
            {
                if (!TryGetUIControl<ProfileCtrl>(uiRoot, "ProfilePanel", out var profileCtrl))
                {
                    Utility.LogDebug("Could not get 'ProfilePanel'");

                    return;
                }

                profileCtrl.m_maidStatus = maid.status;
            }

            static void ApplyAccessoryPlacementControl(Maid maid, GameObject uiRoot)
            {
                if (!TryGetUIControl<SceneEditWindow.CustomPartsWindow>(
                    uiRoot, "Window/CustomPartsWindow", out var sceneEditWindow))
                {
                    Utility.LogDebug("Could not get 'Window/CustomPartsWindow'");

                    return;
                }

                sceneEditWindow.maid = maid;

                // Stopping maid animation and head movement when customizing parts placement
                sceneEditWindow.animation = maid.GetAnimation();
            }

            static bool TryGetUIControl<T>(GameObject root, string hierarchy, out T uiControl)
                where T : MonoBehaviour
            {
                uiControl = null;

                var uiElement = UTY.GetChildObjectNoError(root, hierarchy);

                if (!uiElement)
                    return false;

                uiControl = uiElement.GetComponent<T>();

                return uiControl;
            }
        }
    }

    private void IntegrateWithOkButton()
    {
        if (!SceneEdit.Instance)
            return;

        if (!GetEditOkCancelButton(out var button))
            return;

        EditOkCancel.OnClick newDelegate = RestoreOriginalEditingMaid;

        newDelegate += button.m_dgOnClickOk;

        button.m_dgOnClickOk = newDelegate;
    }

    private void RemoveOkButtonIntegration()
    {
        if (!GetEditOkCancelButton(out var button))
            return;

        Delegate.Remove(button.m_dgOnClickOk, RestoreOriginalEditingMaid);
    }

    private void UpdateOriginalEditingMaid()
    {
        OriginalEditingCharacter = characterRepository.GetByID(SceneEdit.Instance.m_maid.ID());
        EditingCharacter = OriginalEditingCharacter;
    }

    private bool GetEditOkCancelButton(out EditOkCancel button)
    {
        button = null;

        if (!EditMode)
            return false;

        var uiRoot = GameObject.Find("UI Root");

        if (!uiRoot)
            return false;

        var editOkCancelButton = UTY.GetChildObjectNoError(uiRoot, "OkCancel");

        if (!editOkCancelButton)
            return false;

        button = editOkCancelButton.GetComponent<EditOkCancel>();

        return true;
    }
}
