using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class PlacementService(CharacterService characterService)
{
    private const float Pi = Mathf.PI;
    private const float Tau = Mathf.PI * 2f;

    private readonly CharacterService characterService = characterService
        ?? throw new ArgumentNullException(nameof(characterService));

    public enum Placement
    {
        HorizontalRow,
        VerticalRow,
        DiagonalRow,
        DiagonalRowInverse,
        Wave,
        WaveInverse,
        V,
        VInverse,
        CircleInner,
        CircleOuter,
        FanInner,
        FanOuter,
    }

    private IEnumerable<(int Index, Transform Transform)> Transforms =>
        characterService.Select(character => character.GameObject.transform).WithIndex();

    public void ApplyPlacement(Placement placement)
    {
        if (placement is Placement.HorizontalRow)
            RowPlacement(vertical: false);
        else if (placement is Placement.VerticalRow)
            RowPlacement(vertical: true);
        else if (placement is Placement.DiagonalRow)
            DiagonalPlacement(inverse: false);
        else if (placement is Placement.DiagonalRowInverse)
            DiagonalPlacement(inverse: true);
        else if (placement is Placement.Wave)
            WavePlacement(inverse: false);
        else if (placement is Placement.WaveInverse)
            WavePlacement(inverse: true);
        else if (placement is Placement.V)
            VPlacement(inverse: false);
        else if (placement is Placement.VInverse)
            VPlacement(inverse: true);
        else if (placement is Placement.CircleInner)
            CirclePlacement(outer: false);
        else if (placement is Placement.CircleOuter)
            CirclePlacement(outer: true);
        else if (placement is Placement.FanInner)
            FanPlacement(outer: false);
        else if (placement is Placement.FanOuter)
            FanPlacement(outer: true);
        else
            throw new InvalidEnumArgumentException(nameof(placement), (int)placement, typeof(Placement));
    }

    private static int AlternatingSequence(int x) =>
        (int)((x % 2 == 0 ? 1 : -1) * Mathf.Ceil(x / 2f));

    private void RowPlacement(bool vertical = false)
    {
        foreach (var (i, transform) in Transforms)
        {
            var a = AlternatingSequence(i) * 0.5f;

            transform.localPosition = vertical ? new(0f, 0f, a) : new(a, 0f, 0f);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void DiagonalPlacement(bool inverse = false)
    {
        foreach (var (i, transform) in Transforms)
        {
            var a = AlternatingSequence(i) * 0.5f;

            transform.localPosition = inverse ? new(a, 0f, -a) : new(a, 0, a);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void WavePlacement(bool inverse = false)
    {
        foreach (var (i, transform) in Transforms)
        {
            var x = AlternatingSequence(i) * 0.4f;
            var z = (inverse ? -1 : 1) * Mathf.Cos(AlternatingSequence(i) * Pi) * 0.35f;

            transform.localPosition = new(x, 0f, z);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void VPlacement(bool inverse = false)
    {
        foreach (var (i, transform) in Transforms)
        {
            var x = AlternatingSequence(i) * 0.4f;
            var z = (inverse ? 1 : -1) * Mathf.Abs(AlternatingSequence(i)) * 0.4f;

            transform.localPosition = new(x, 0, z);
            transform.localRotation = Quaternion.identity;
        }
    }

    private void CirclePlacement(bool outer = false)
    {
        var radius = characterService.Count * 0.1f + 0.3f;

        foreach (var (i, transform) in Transforms)
        {
            var angle = Pi / 2f + Tau * AlternatingSequence(i) / characterService.Count;
            var x = Mathf.Cos(angle) * radius;
            var z = Mathf.Sin(angle) * radius;

            var rotation = Mathf.Atan2(x, z);

            if (!outer)
                rotation += Pi;

            transform.localPosition = new(x, 0f, z);
            transform.localRotation = Quaternion.Euler(0f, rotation * Mathf.Rad2Deg, 0f);
        }
    }

    private void FanPlacement(bool outer = false)
    {
        var radius = characterService.Count * 0.2f + 0.2f;

        SetPositionAndRotation(characterService[0].GameObject.transform, Vector3.zero, Quaternion.identity);

        foreach (var (i, transform) in Transforms.Skip(1))
        {
            var angle = Pi * AlternatingSequence(i - 1) / characterService.Count;
            var x = Mathf.Sin(angle) * radius;
            var z = Mathf.Cos(angle) * radius;
            var rotation = Mathf.Atan2(x, z);

            if (outer)
                rotation += Pi;

            SetPositionAndRotation(transform, new(-x, 0, -z), Quaternion.Euler(0, rotation * Mathf.Rad2Deg, 0));
        }

        static void SetPositionAndRotation(Transform transform, Vector3 position, Quaternion rotation)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }
    }
}
