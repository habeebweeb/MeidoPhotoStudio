using System;
using System.Collections.Generic;
using System.Linq;

using MyRoomCustom;
using UnityEngine;

namespace MeidoPhotoStudio.Converter.MultipleMaids;

public static class MMConstants
{
    public static readonly Vector3 DefaultSoftG = new(0f, -3f / 1000f, 0f);

    public static readonly string[] FaceKeys =
    {
        "eyeclose", "eyeclose2", "eyeclose3", "eyeclose6", "hitomih", "hitomis", "mayuha", "mayuup", "mayuv",
        "mayuvhalf", "moutha", "mouths", "mouthdw", "mouthup", "tangout", "tangup", "eyebig", "eyeclose5", "mayuw",
        "mouthhe", "mouthc", "mouthi", "mouthuphalf", "tangopen", "namida", "tear1", "tear2", "tear3", "shock",
        "yodare", "hoho", "hoho2", "hohos", "hohol", "toothoff", "nosefook",
    };

    public static readonly string[] MpnAttachProps =
    {
        // NOTE: MPS only allows a subset of attached MPN props because MPS has a better method of attaching props.
        // "", "", "", "", "", "", "", "", "",
        "kousokuu_tekaseone_i_.menu", "kousokuu_tekasetwo_i_.menu", "kousokul_ashikaseup_i_.menu",
        "kousokuu_tekasetwo_i_.menu", "kousokul_ashikasedown_i_.menu", "kousokuu_tekasetwodown_i_.menu",
        "kousokuu_ushirode_i_.menu", "kousokuu_smroom_haritsuke_i_.menu",
    };

    private static Dictionary<string, PlacementData.Data>? myrAssetNameToData;

    public static Dictionary<string, PlacementData.Data> MyrAssetNameToData =>
        myrAssetNameToData ??= PlacementData.GetAllDatas(false)
            .ToDictionary(
                data => string.IsNullOrEmpty(data.assetName) ? data.resourceName : data.assetName,
                data => data,
                StringComparer.InvariantCultureIgnoreCase);
}
