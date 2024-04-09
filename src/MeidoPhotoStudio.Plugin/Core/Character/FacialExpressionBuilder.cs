using System;
using System.Linq;

using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FacialExpressionBuilder(FaceShapeKeyConfiguration faceShapeKeyConfiguration)
{
    private static readonly string[] StockHashes =
    [
        "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "eyeclose8", "eyeclose7", "hitomih",
        "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths", "mouthc", "mouthi", "mouthup",
        "mouthdw", "mouthhe", "mouthuphalf", "tangout", "tangup", "tangopen", "hoho2", "shock", "nosefook", "namida",
        "yodare", "toothoff", "tear1", "tear2", "tear3", "hohos", "hoho", "hohol",
    ];

    private readonly FaceShapeKeyConfiguration faceShapeKeyConfiguration = faceShapeKeyConfiguration
        ?? throw new ArgumentNullException(nameof(faceShapeKeyConfiguration));

    public FacialExpressionSet Build(FaceController face) =>
        face.GetFaceData(StockHashes.Concat(faceShapeKeyConfiguration.CustomShapeKeys));
}
