using System.Collections.Generic;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class MaidPlacementUtility
{
    public static readonly string[] PlacementTypes =
    {
        "horizontalRow", "verticalRow", "diagonalRow", "diagonalRowInverse", "wave", "waveInverse", "v", "vInverse",
        "circleInner", "circleOuter", "fanInner", "fanOuter",
    };

    private const float Pi = Mathf.PI;
    private const float Tau = Mathf.PI * 2f;

    public static int AlternatingSequence(int x) =>
        (int)((x % 2 == 0 ? 1 : -1) * Mathf.Ceil(x / 2f));

    public static void ApplyPlacement(string placementType, IList<Meido> list)
    {
        switch (placementType)
        {
            case "horizontalRow":
                PlacementRow(list, false);

                break;
            case "verticalRow":
                PlacementRow(list, true);

                break;
            case "diagonalRow":
                PlacementDiagonal(list, false);

                break;
            case "diagonalRowInverse":
                PlacementDiagonal(list, true);

                break;
            case "wave":
                PlacementWave(list, false);

                break;
            case "waveInverse":
                PlacementWave(list, true);

                break;
            case "v":
                PlacementV(list, false);

                break;
            case "vInverse":
                PlacementV(list, true);

                break;
            case "circleOuter":
                PlacementCircle(list, false);

                break;
            case "circleInner":
                PlacementCircle(list, true);

                break;
            case "fanInner":
                PlacementFan(list, false);

                break;
            case "fanOuter":
                PlacementFan(list, true);

                break;
            default:
                return;
        }
    }

    public static void PlacementRow(IList<Meido> list, bool vertical = false)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var position = Vector3.zero;
            var maid = list[i].Maid;
            var a = AlternatingSequence(i) * 0.6f;

            if (vertical)
                position.z = a;
            else
                position.x = a;

            maid.SetPos(position);
            maid.SetRot(Vector3.zero);
        }
    }

    public static void PlacementDiagonal(IList<Meido> list, bool inverse = false)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var maid = list[i].Maid;

            var z = AlternatingSequence(i) * 0.5f;

            maid.SetPos(inverse ? new(z, 0, -z) : new(z, 0, z));
            maid.SetRot(Vector3.zero);
        }
    }

    public static void PlacementWave(IList<Meido> list, bool inverse = false)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var maid = list[i].Maid;
            var x = AlternatingSequence(i) * 0.4f;
            var z = (inverse ? -1 : 1) * Mathf.Cos(AlternatingSequence(i) * Pi) * 0.35f;

            maid.SetPos(new(x, 0, z));
            maid.SetRot(Vector3.zero);
        }
    }

    public static void PlacementV(IList<Meido> list, bool inverse = false)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var maid = list[i].Maid;
            var x = AlternatingSequence(i) * 0.4f;
            var z = (inverse ? 1 : -1) * Mathf.Abs(AlternatingSequence(i)) * 0.4f;

            maid.SetPos(new(x, 0, z));
            maid.SetRot(Vector3.zero);
        }
    }

    public static void PlacementCircle(IList<Meido> list, bool inner = false)
    {
        var maidCount = list.Count;

        var radius = 0.3f + 0.1f * maidCount;

        for (var i = 0; i < maidCount; i++)
        {
            var maid = list[i].Maid;
            var angle = Pi / 2f + Tau * AlternatingSequence(i) / maidCount;
            var x = Mathf.Cos(angle) * radius;
            var z = Mathf.Sin(angle) * radius;

            var rotation = Mathf.Atan2(x, z);

            if (inner)
                rotation += Pi;

            maid.SetPos(new(x, 0, z));
            maid.SetRot(new(0, rotation * Mathf.Rad2Deg, 0));
        }
    }

    public static void PlacementFan(IList<Meido> list, bool outer = false)
    {
        var maidCount = list.Count;
        var radius = 0.2f + 0.2f * maidCount;

        list[0].Maid.SetPos(Vector3.zero);
        list[0].Maid.SetRot(Vector3.zero);

        for (var i = 1; i < maidCount; i++)
        {
            var maid = list[i].Maid;
            var angle = Pi * AlternatingSequence(i - 1) / maidCount;
            var x = Mathf.Sin(angle) * radius;
            var z = Mathf.Cos(angle) * radius;
            var rotation = Mathf.Atan2(x, z);

            if (outer)
                rotation += Pi;

            maid.SetPos(new(-x, 0, -z));
            maid.SetRot(new(0, rotation * Mathf.Rad2Deg, 0));
        }
    }
}
