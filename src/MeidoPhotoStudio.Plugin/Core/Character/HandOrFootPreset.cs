using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HandOrFootPreset : IEnumerable<Quaternion>
{
    private readonly Quaternion[] rotations;

    public HandOrFootPreset(IEnumerable<Quaternion> rotations, HandOrFootType type)
    {
        this.rotations = rotations.ToArray();

        if (!Enum.IsDefined(typeof(HandOrFootType), type))
            throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(HandOrFootType));

        Type = type;
    }

    public HandOrFootType Type { get; }

    public bool FromRight =>
        Type is HandOrFootType.HandRight or HandOrFootType.FootRight;

    public bool IsHandPreset =>
        Type is HandOrFootType.HandLeft or HandOrFootType.HandRight;

    public IEnumerable<Quaternion> LeftRotations =>
        GetRotations(rightRotations: false);

    public IEnumerable<Quaternion> RightRotations =>
        GetRotations(rightRotations: true);

    public IEnumerator<Quaternion> GetEnumerator() =>
        ((IEnumerable<Quaternion>)rotations).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private IEnumerable<Quaternion> GetRotations(bool rightRotations) =>
        rightRotations == FromRight
            ? rotations
            : rotations.Select(rotation => rotation with { x = rotation.x * -1, y = rotation.y * -1 });
}
