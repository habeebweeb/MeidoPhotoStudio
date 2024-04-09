using System;
using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Framework.Extensions;

using UnityEngine;

namespace MeidoPhotoStudio.Database.Character;

public class CharacterModel(Maid maid) : IEquatable<CharacterModel>
{
    public Maid Maid { get; } = maid ? maid : throw new ArgumentNullException(nameof(maid));

    public string ID =>
        Maid.status.guid;

    public string FirstName =>
        Maid.status.firstName;

    public string LastName =>
        Maid.status.lastName;

    public Texture2D Portrait =>
        Maid.GetThumIcon();

    public string FullName() =>
        FullName("{0} {1}");

    public string FullName(string format)
    {
        if (string.IsNullOrEmpty(format))
            format = "{0} {1}";

        return string.Format(format, FirstName, LastName);
    }

    public override bool Equals(object other) =>
        Equals(other as CharacterModel);

    public bool Equals(CharacterModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Maid.ValueEquals(other.Maid);
    }

    public override int GetHashCode()
    {
        var hashCode = -1348474836;

        hashCode = hashCode * -1521134295 + EqualityComparer<Maid>.Default.GetHashCode(Maid);

        return hashCode;
    }

    public override string ToString() =>
        $"'{FirstName} {LastName}' ({ID})";
}
