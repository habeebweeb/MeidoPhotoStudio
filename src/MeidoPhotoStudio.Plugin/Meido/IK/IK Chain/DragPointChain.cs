using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPointChain : DragPointMeido
{
    protected readonly TBody.IKCMO IK = new();
    protected readonly Quaternion[] jointRotation = new Quaternion[3];

    // WARN: This does NOT work and is only done so the compiler does not complain
    protected
#if COM25
    AIKCtrl
#else
    IKCtrlData
#endif
    ikCtrlData;

    protected Transform[] ikChain;

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        ikChain = new Transform[]
        {
            myObject.parent,
            myObject.parent,
            myObject,
        };

        ikCtrlData = IkCtrlData;
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        InitializeRotation();

        InitializeIK(IK, ikChain[JointUpper], ikChain[JointMiddle], ikChain[JointLower]);
    }

    protected void InitializeRotation()
    {
        for (var i = 0; i < jointRotation.Length; i++)
            jointRotation[i] = ikChain[i].localRotation;
    }
}
