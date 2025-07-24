using Microsoft.CodeAnalysis;

namespace COM3D2X.SafeEnum;

internal readonly record struct SafeEnumModel
{
    public string? Namespace { get; init; }

    public Accessibility Accessibility { get; init; }

    public string? Name { get; init; }

    public string? EnumType { get; init; }

    public ValueCollection<string> EnumMembers { get; init; }
}
