using System;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class TMorphExtensions
{
    public static string GP01FbFaceHashKey(this TMorph face, string hashKey)
    {
        if (face.bodyskin.body.Face.morph != face)
            throw new InvalidOperationException("Morph is not a face morph.");

        if (face.bodyskin.PartsVersion < 120)
            return hashKey;

        if (string.Equals(hashKey, "eyeclose3", StringComparison.OrdinalIgnoreCase))
            return hashKey;

        if (!hashKey.StartsWith("eyeclose", StringComparison.OrdinalIgnoreCase))
            return hashKey;

        var gp01FbHash = hashKey;

        if (string.Equals(hashKey, "eyeclose"))
            gp01FbHash += '1';

        gp01FbHash += TMorph.crcFaceTypesStr[(int)face.GetFaceTypeGP01FB()];

        return gp01FbHash;
    }
}
