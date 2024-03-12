using System;
using System.Collections.Generic;
using System.Linq;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

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

    private readonly Dictionary<PropController, AttachPointInfo> attachedProps = new();
    private readonly MeidoManager meidoManager;
    private readonly PropService propService;

    public PropAttachmentService(MeidoManager meidoManager, PropService propService)
    {
        this.meidoManager = meidoManager ?? throw new ArgumentNullException(nameof(meidoManager));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));

        this.meidoManager.BeginCallMeidos += OnLoadingMeido;
        this.meidoManager.EndCallMeidos += OnLoadedMeido;

        this.propService.RemovedProp += OnRemovedProp;
    }

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

    public void AttachPropTo(PropController prop, Meido meido, AttachPoint attachPoint, bool keepPosition)
    {
        AttachProp(prop, meido, attachPoint, keepPosition);

        attachedProps[prop] = new(attachPoint, meido);
    }

    public void DetachProp(PropController prop)
    {
        if (!attachedProps.ContainsKey(prop))
            return;

        var propTransform = prop.GameObject.transform;
        var originalScale = propTransform.localScale;

        propTransform.SetParent(null, true);

        propTransform.localScale = originalScale;

        attachedProps.Remove(prop);
    }

    private void AttachProp(PropController prop, Meido meido, AttachPoint attachPoint, bool keepPosition)
    {
        var attachTransform = meido.Body.GetBone(AttachPointToBoneName[attachPoint]);
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

    private void OnLoadingMeido(object sender, EventArgs e)
    {
        foreach (var attachedProp in attachedProps.Keys.Select(prop => prop.GameObject.transform))
            attachedProp.SetParent(null, true);
    }

    private void OnLoadedMeido(object sender, EventArgs e)
    {
        foreach (var (prop, attachInfo) in attachedProps.ToArray())
        {
            var meido = meidoManager.GetMeido(attachInfo.MaidGuid);

            if (meido is null)
            {
                attachedProps.Remove(prop);

                continue;
            }

            AttachProp(prop, meido, attachInfo.AttachPoint, true);
        }
    }

    private void OnRemovedProp(object sender, PropServiceEventArgs e)
    {
        if (!attachedProps.ContainsKey(e.PropController))
            return;

        attachedProps.Remove(e.PropController);
    }
}
