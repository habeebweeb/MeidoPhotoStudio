using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal static class MaidPlacementUtility
    {
        private static readonly float pi = Mathf.PI;
        private static readonly float tau = Mathf.PI * 2f;
        public static readonly string[] placementTypes = {
            "horizontalRow", "verticalRow", "diagonalRow", "diagonalRowInverse", "wave", "waveInverse",
            "v", "vInverse", "circleInner", "circleOuter", "fanInner", "fanOuter"
        };

        public static int AlternatingSequence(int x)
        {
            return (int)((x % 2 == 0 ? 1 : -1) * Mathf.Ceil(x / 2f));
        }

        public static void ApplyPlacement(string placementType, IList<Meido> list)
        {
            switch (placementType)
            {
                case "horizontalRow": PlacementRow(list, false); break;
                case "verticalRow": PlacementRow(list, true); break;
                case "diagonalRow": PlacementDiagonal(list, false); break;
                case "diagonalRowInverse": PlacementDiagonal(list, true); break;
                case "wave": PlacementWave(list, false); break;
                case "waveInverse": PlacementWave(list, true); break;
                case "v": PlacementV(list, false); break;
                case "vInverse": PlacementV(list, true); break;
                case "circleOuter": PlacementCircle(list, false); break;
                case "circleInner": PlacementCircle(list, true); break;
                case "fanInner": PlacementFan(list, false); break;
                case "fanOuter": PlacementFan(list, true); break;
                default: return;
            }
        }

        public static void PlacementRow(IList<Meido> list, bool vertical = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Vector3 position = Vector3.zero;

                Maid maid = list[i].Maid;

                float a = AlternatingSequence(i) * 0.6f;

                if (vertical) position.z = a;
                else position.x = a;

                maid.SetPos(position);
                maid.SetRot(Vector3.zero);
            }
        }

        public static void PlacementDiagonal(IList<Meido> list, bool inverse = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Maid maid = list[i].Maid;

                float z = AlternatingSequence(i) * 0.5f;

                maid.SetPos(inverse ? new Vector3(z, 0, -z) : new Vector3(z, 0, z));
                maid.SetRot(Vector3.zero);
            }
        }

        public static void PlacementWave(IList<Meido> list, bool inverse = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Maid maid = list[i].Maid;

                float x = AlternatingSequence(i) * 0.4f;
                float z = (inverse ? -1 : 1) * Mathf.Cos(AlternatingSequence(i) * pi) * 0.35f;

                maid.SetPos(new Vector3(x, 0, z));
                maid.SetRot(Vector3.zero);
            }
        }

        public static void PlacementV(IList<Meido> list, bool inverse = false)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Maid maid = list[i].Maid;

                float x = AlternatingSequence(i) * 0.4f;
                float z = (inverse ? 1 : -1) * Mathf.Abs(AlternatingSequence(i)) * 0.4f;

                maid.SetPos(new Vector3(x, 0, z));
                maid.SetRot(Vector3.zero);
            }
        }

        public static void PlacementCircle(IList<Meido> list, bool inner = false)
        {
            int maidCount = list.Count;

            float radius = (0.3f + 0.1f * maidCount);

            for (int i = 0; i < maidCount; i++)
            {
                Maid maid = list[i].Maid;

                float angle = (pi / 2f) + tau * AlternatingSequence(i) / maidCount;

                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                float rotation = Mathf.Atan2(x, z);
                if (inner) rotation += pi;

                maid.SetPos(new Vector3(x, 0, z));
                maid.SetRot(new Vector3(0, rotation * Mathf.Rad2Deg, 0));
            }
        }

        public static void PlacementFan(IList<Meido> list, bool outer = false)
        {
            int maidCount = list.Count;

            float radius = (0.2f + 0.2f * maidCount);

            list[0].Maid.SetPos(Vector3.zero);
            list[0].Maid.SetRot(Vector3.zero);

            for (int i = 1; i < maidCount; i++)
            {
                Maid maid = list[i].Maid;

                float angle = pi * AlternatingSequence(i - 1) / maidCount;

                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;

                float rotation = Mathf.Atan2(x, z);
                if (outer) rotation += pi;

                maid.SetPos(new Vector3(-x, 0, -z));
                maid.SetRot(new Vector3(0, rotation * Mathf.Rad2Deg, 0));
            }
        }
    }
}
