using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SlotID = TBody.SlotID;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class ClothingSet : IEnumerable<KeyValuePair<SlotID, bool>>
{
    private readonly Dictionary<SlotID, bool> clothingStates;

    public ClothingSet() =>
        clothingStates = [];

    public ClothingSet(IDictionary<SlotID, bool> clothingStates)
    {
        _ = clothingStates ?? throw new System.ArgumentException(nameof(clothingStates));
        this.clothingStates = clothingStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public ClothingSet(IEnumerable<KeyValuePair<SlotID, bool>> clothingStates)
    {
        _ = clothingStates ?? throw new System.ArgumentException(nameof(clothingStates));
        this.clothingStates = clothingStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public bool this[SlotID slot] =>
        clothingStates[slot];

    public bool ContainsSlot(SlotID slot) =>
        clothingStates.ContainsKey(slot);

    public bool TryGetClothingState(SlotID slot, out bool value) =>
        clothingStates.TryGetValue(slot, out value);

    public IEnumerator<KeyValuePair<SlotID, bool>> GetEnumerator() =>
        clothingStates.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
