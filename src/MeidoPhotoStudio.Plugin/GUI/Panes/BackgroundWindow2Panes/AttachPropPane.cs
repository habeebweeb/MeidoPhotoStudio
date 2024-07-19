using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class AttachPropPane : BasePane
{
    private static readonly Dictionary<AttachPoint, string> ToggleTranslation =
        new()
        {
            [AttachPoint.Head] = "head",
            [AttachPoint.Neck] = "neck",
            [AttachPoint.UpperArmL] = "upperArmL",
            [AttachPoint.UpperArmR] = "upperArmR",
            [AttachPoint.ForearmL] = "forearmL",
            [AttachPoint.ForearmR] = "forearmR",
            [AttachPoint.MuneL] = "muneL",
            [AttachPoint.MuneR] = "muneR",
            [AttachPoint.HandL] = "handL",
            [AttachPoint.HandR] = "handR",
            [AttachPoint.Pelvis] = "pelvis",
            [AttachPoint.ThighL] = "thighL",
            [AttachPoint.ThighR] = "thighR",
            [AttachPoint.CalfL] = "calfL",
            [AttachPoint.CalfR] = "calfR",
            [AttachPoint.FootL] = "footL",
            [AttachPoint.FootR] = "footR",
            [AttachPoint.Spine1a] = "spine1a",
            [AttachPoint.Spine1] = "spine1",
            [AttachPoint.Spine0a] = "spine0a",
            [AttachPoint.Spine0] = "spine0",
        };

    private static readonly AttachPoint[][] AttachPointGroups =
    [
        [AttachPoint.Head, AttachPoint.Neck],
        [AttachPoint.UpperArmR, AttachPoint.Spine1a, AttachPoint.UpperArmL],
        [AttachPoint.ForearmR, AttachPoint.Spine1, AttachPoint.ForearmL],
        [AttachPoint.MuneR, AttachPoint.Spine0a, AttachPoint.MuneL],
        [AttachPoint.HandR, AttachPoint.Spine0, AttachPoint.HandL],
        [AttachPoint.ThighR, AttachPoint.Pelvis, AttachPoint.ThighL],
        [AttachPoint.CalfR, AttachPoint.CalfL],
        [AttachPoint.FootR, AttachPoint.FootL]
    ];

    private readonly CharacterService characterService;
    private readonly PropAttachmentService propAttachmentService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly Dropdown characterDropdown;
    private readonly Dictionary<AttachPoint, Toggle> attachPointToggles = new(EnumEqualityComparer<AttachPoint>.Instance);
    private readonly Toggle keepWorldPositionToggle;
    private readonly PaneHeader paneHeader;

    public AttachPropPane(
        CharacterService characterService,
        PropAttachmentService propAttachmentService,
        SelectionController<PropController> propSelectionController)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.propAttachmentService = propAttachmentService ?? throw new ArgumentNullException(nameof(propAttachmentService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;
        this.propSelectionController.Selected += OnCharacterOrPropSelected;
        this.propAttachmentService.AttachedProp += OnPropAttachedOrDetached;
        this.propAttachmentService.DetachedProp += OnPropAttachedOrDetached;

        paneHeader = new(Translation.Get("attachPropPane", "header"), true);

        characterDropdown = new([Translation.Get("systemMessage", "noMaids")]);
        characterDropdown.SelectionChange += OnCharacterOrPropSelected;

        keepWorldPositionToggle = new(Translation.Get("attachPropPane", "keepWorldPosition"));

        foreach (var attachPoint in Enum.GetValues(typeof(AttachPoint))
            .Cast<AttachPoint>()
            .Where(attachPoint => attachPoint is not AttachPoint.None))
        {
            var toggle = new Toggle(Translation.Get("attachPropPane", ToggleTranslation[attachPoint]));

            toggle.ControlEvent += (object sender, EventArgs e) =>
                OnToggleChanged(attachPoint);

            attachPointToggles[attachPoint] = toggle;
        }
    }

    private PropController CurrentProp =>
        propSelectionController.Current;

    private CharacterController CurrentCharacter =>
        characterService.Count > 0
            ? characterService[characterDropdown.SelectedItemIndex]
            : null;

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUI.enabled = characterService.Count > 0 && CurrentProp is not null;

        DrawCharacterDropdown();

        MpsGui.BlackLine();

        keepWorldPositionToggle.Draw();

        MpsGui.BlackLine();

        foreach (var attachPointGroup in AttachPointGroups)
            DrawToggleGroup(attachPointGroup);

        GUI.enabled = true;

        void DrawCharacterDropdown()
        {
            GUILayout.BeginHorizontal();
            characterDropdown.Draw(GUILayout.Width(185f));

            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            if (GUILayout.Button("<", arrowLayoutOptions))
                characterDropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                characterDropdown.Step(1);

            GUILayout.EndHorizontal();
        }

        void DrawToggleGroup(AttachPoint[] attachPoints)
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            foreach (var point in attachPoints)
                attachPointToggles[point].Draw();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("attachPropPane", "header");
        keepWorldPositionToggle.Label = Translation.Get("attachPropPane", "keepWorldPosition");

        foreach (var attachPoint in Enum.GetValues(typeof(AttachPoint)).Cast<AttachPoint>())
        {
            if (attachPoint is AttachPoint.None)
                continue;

            attachPointToggles[attachPoint].Label = Translation.Get("attachPropPane", ToggleTranslation[attachPoint]);
        }
    }

    private void UpdateToggles()
    {
        if (CurrentProp is null || CurrentCharacter is null)
            return;

        if (propAttachmentService.TryGetAttachPointInfo(CurrentProp, out var attachPointInfo))
        {
            var attachedToggle = attachPointToggles[attachPointInfo.AttachPoint];

            attachedToggle.SetEnabledWithoutNotify(
                string.Equals(CurrentCharacter.ID, attachPointInfo.MaidGuid, StringComparison.OrdinalIgnoreCase));

            var otherEnabledToggles = attachPointToggles.Values
                .Where(toggle => toggle != attachedToggle)
                .Where(toggle => toggle.Value);

            foreach (var toggle in otherEnabledToggles)
                toggle.SetEnabledWithoutNotify(false);
        }
        else
        {
            foreach (var toggle in attachPointToggles.Values.Where(toggle => toggle.Value))
                toggle.SetEnabledWithoutNotify(false);
        }
    }

    private void OnToggleChanged(AttachPoint point)
    {
        if (CurrentProp is null || CurrentCharacter is null)
            return;

        var changedToggle = attachPointToggles[point];

        if (changedToggle.Value)
        {
            propAttachmentService.AttachPropTo(CurrentProp, CurrentCharacter, point, keepWorldPositionToggle.Value);

            var otherEnabledToggles = attachPointToggles.Values
                .Where(toggle => toggle != changedToggle)
                .Where(toggle => toggle.Value);

            foreach (var toggle in otherEnabledToggles)
                toggle.SetEnabledWithoutNotify(false);
        }
        else
        {
            propAttachmentService.DetachProp(CurrentProp);
        }
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        var dropdownList = characterService
            .Select(character => $"{character.Slot + 1}: {character.CharacterModel.FullName()}")
            .ToArray();

        if (!dropdownList.Any())
            dropdownList = [Translation.Get("systemMessage", "noMaids")];

        characterDropdown.SetDropdownItems(dropdownList, 0);
    }

    private void OnCharacterOrPropSelected(object sender, EventArgs e) =>
        UpdateToggles();

    private void OnPropAttachedOrDetached(object sender, PropAttachmentEventArgs e)
    {
        if (!attachPointToggles.TryGetValue(e.AttachPoint, out var toggle))
            return;

        if (e.Character != CurrentCharacter)
            return;

        if (e.Prop != CurrentProp)
            return;

        toggle.SetEnabledWithoutNotify(propAttachmentService.PropIsAttached(e.Prop));
    }
}
