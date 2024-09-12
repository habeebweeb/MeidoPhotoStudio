using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropAttachmentService
{
    private static readonly Dictionary<AttachPoint, string> AttachPointToBoneName = new()
    {
        [AttachPoint.Head] = "Bip01 Head",
        [AttachPoint.Neck] = "Bip01 Neck",
        [AttachPoint.UpperArmL] = "_IK_UpperArmL",
        [AttachPoint.UpperArmR] = "_IK_UpperArmR",
        [AttachPoint.ForearmL] = "_IK_ForeArmL",
        [AttachPoint.ForearmR] = "_IK_ForeArmR",
        [AttachPoint.MuneL] = "_IK_muneL",
        [AttachPoint.MuneR] = "_IK_muneR",
        [AttachPoint.HandL] = "_IK_handL",
        [AttachPoint.HandR] = "_IK_handR",
        [AttachPoint.Pelvis] = "Bip01 Pelvis",
        [AttachPoint.ThighL] = "_IK_thighL",
        [AttachPoint.ThighR] = "_IK_thighR",
        [AttachPoint.CalfL] = "_IK_calfL",
        [AttachPoint.CalfR] = "_IK_calfR",
        [AttachPoint.FootL] = "_IK_footL",
        [AttachPoint.FootR] = "_IK_footR",
        [AttachPoint.Spine1a] = "Bip01 Spine1a",
        [AttachPoint.Spine1] = "Bip01 Spine1",
        [AttachPoint.Spine0a] = "Bip01 Spine0a",
        [AttachPoint.Spine0] = "Bip01 Spine",
    };

    private readonly Dictionary<PropController, AttachPointInfo> attachedProps = [];
    private readonly CharacterService characterService;

    private readonly PropService propService;

    public PropAttachmentService(CharacterService characterService, PropService propService)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));

        this.characterService.CallingCharacters += OnCallingCharacters;
        this.characterService.CalledCharacters += OnCalledCharacters;
        this.characterService.Deactivating += OnDeactivating;

        this.propService.RemovedProp += OnPropRemoved;
    }

    public event EventHandler<PropAttachmentEventArgs> AttachedProp;

    public event EventHandler<PropAttachmentEventArgs> DetachedProp;

    public AttachPointInfo this[PropController propController] =>
        propController is null
            ? throw new ArgumentNullException(nameof(propController))
            : attachedProps[propController];

    public bool TryGetAttachPointInfo(PropController propController, out AttachPointInfo attachPointInfo) =>
        attachedProps.TryGetValue(propController, out attachPointInfo);

    public bool PropIsAttached(PropController propController) =>
        propController is null
            ? throw new ArgumentNullException(nameof(propController))
            : attachedProps.ContainsKey(propController);

    public void AttachPropTo(PropController prop, CharacterController character, AttachPoint attachPoint, bool keepPosition)
    {
        _ = prop ?? throw new ArgumentNullException(nameof(prop));
        _ = character ?? throw new ArgumentNullException(nameof(character));

        if (!Enum.IsDefined(typeof(AttachPoint), attachPoint))
            throw new InvalidEnumArgumentException(nameof(attachPoint), (int)attachPoint, typeof(AttachPoint));

        AttachProp(prop, character, attachPoint, keepPosition);

        attachedProps[prop] = new(attachPoint, character);

        AttachedProp?.Invoke(this, new(prop, character, attachPoint));
    }

    public void DetachProp(PropController prop)
    {
        _ = prop ?? throw new ArgumentNullException(nameof(prop));

        if (!attachedProps.TryGetValue(prop, out var attachPointInfo))
            return;

        var propTransform = prop.GameObject.transform;
        var originalScale = propTransform.localScale;

        propTransform.SetParent(null, true);

        propTransform.localScale = originalScale;

        SceneManager.MoveGameObjectToScene(propTransform.gameObject, SceneManager.GetActiveScene());

        attachedProps.Remove(prop);

        DetachedProp?.Invoke(
            this,
            new(
                prop,
                characterService.GetCharacterControllerByID(attachPointInfo.MaidGuid),
                attachPointInfo.AttachPoint));
    }

    private void AttachProp(PropController prop, CharacterController character, AttachPoint attachPoint, bool keepPosition)
    {
        var attachTransform = character.IK.GetBone(AttachPointToBoneName[attachPoint]);
        var propTransform = prop.GameObject.transform;

        var rotation = propTransform.rotation;
        var localScale = propTransform.localScale;

        propTransform.SetParent(attachTransform, keepPosition);

        if (keepPosition)
        {
            propTransform.rotation = rotation;
        }
        else
        {
            propTransform.localPosition = Vector3.zero;
            propTransform.rotation = Quaternion.identity;
        }

        propTransform.localScale = localScale;
    }

    private void OnCallingCharacters(object sender, CharacterServiceEventArgs e)
    {
        foreach (var attachedProp in attachedProps.Keys.Select(prop => prop.GameObject.transform))
            attachedProp.SetParent(null, true);
    }

    private void OnCalledCharacters(object sender, CharacterServiceEventArgs e)
    {
        foreach (var (prop, attachInfo) in attachedProps.ToArray())
        {
            var character = characterService.GetCharacterControllerByID(attachInfo.MaidGuid);

            if (character is null)
            {
                attachedProps.Remove(prop);

                continue;
            }

            AttachProp(prop, character, attachInfo.AttachPoint, true);
        }
    }

    private void OnPropRemoved(object sender, PropServiceEventArgs e)
    {
        if (!attachedProps.ContainsKey(e.PropController))
            return;

        attachedProps.Remove(e.PropController);
    }

    private void OnDeactivating(object sender, EventArgs e)
    {
        foreach (var prop in attachedProps.Keys.Select(prop => prop.GameObject.transform))
            prop.SetParent(null, true);
    }
}
