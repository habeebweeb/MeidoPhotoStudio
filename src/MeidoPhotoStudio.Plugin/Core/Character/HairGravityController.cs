namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HairGravityController(CharacterController characterController)
    : GravityController(characterController)
{
    protected override string TypeName =>
        "Hair";

    protected override void InitializeTransformControl(GravityTransformControl control)
    {
        control.forceRate = 0.1f;

        control.SetTargetSlods(character.Maid.body0.goSlot
            .Where(slot => slot.obj)
            .Where(slot => slot.obj.GetComponent<DynamicBone>())
            .Select(slot => slot.SlotId)
            .ToArray());
    }
}
