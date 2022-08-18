using UnityEngine;

using static TBody;

namespace MeidoPhotoStudio.Plugin;

public class DragPointGravity : DragPointGeneral
{
    private static readonly SlotID[] SkirtSlots = { SlotID.skirt, SlotID.onepiece, SlotID.mizugi, SlotID.panz };
    private static readonly SlotID[] HairSlots = { SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT };

    public GravityTransformControl Control { get; private set; }

    public bool Valid =>
        Control.isValid;

    public bool Active =>
        Valid && gameObject.activeSelf;

    public static GravityTransformControl MakeGravityControl(Maid maid, bool skirt = false)
    {
        var category = skirt ? "skirt" : "hair";
        var bone = maid.body0.GetBone("Bip01");
        var gravityGoName = $"GravityDatas_{maid.status.guid}_{category}";
        var gravityTransform = maid.gameObject.transform.Find(gravityGoName);

        if (!gravityTransform)
        {
            var go = new GameObject(gravityGoName);

            go.transform.SetParent(bone, false);
            go.transform.SetParent(maid.transform, true);
            go.transform.localScale = Vector3.one;
            go.transform.rotation = Quaternion.identity;

            var go2 = new GameObject(gravityGoName);

            go2.transform.SetParent(go.transform, false);
            gravityTransform = go2.transform;
        }
        else
        {
            gravityTransform = gravityTransform.GetChild(0);

            var control = gravityTransform.GetComponent<GravityTransformControl>();

            if (control)
                Destroy(control);
        }

        var gravityControl = gravityTransform.gameObject.AddComponent<GravityTransformControl>();
        var slots = skirt ? SkirtSlots : HairSlots;

        gravityControl.SetTargetSlods(slots);
        gravityControl.forceRate = 0.1f;

        return gravityControl;
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        Control = myObject.GetComponent<GravityTransformControl>();
        gameObject.SetActive(false);
    }

    protected override void ResetPosition() =>
        Control.transform.localPosition = DefaultPosition;

    protected override void ApplyDragType()
    {
        ApplyProperties(Moving, Moving, false);
        ApplyColours();
    }

    protected override void OnDestroy()
    {
        if (Control.isValid)
        {
            Control.transform.localPosition = Vector3.zero;
            Control.Update();
        }

        Destroy(Control.transform.parent.gameObject);

        base.OnDestroy();
    }

    private void OnDisable() =>
        Control.isEnabled = false;

    private void OnEnable()
    {
        if (!Control)
            return;

        // TODO: WTF?
        Control.isEnabled = true;

        if (!Control.isEnabled)
            gameObject.SetActive(false);
    }
}
