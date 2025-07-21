using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

readonly struct UsingDirectiveMeta : IEquatable<UsingDirectiveMeta>
{
    public readonly bool UseVContainer;
    public readonly string? AliasName;

    public UsingDirectiveMeta(UsingDirectiveSyntax usingDirective)
    {
        UseVContainer = usingDirective.Alias == null;
        AliasName = UseVContainer ? "" : usingDirective.Alias?.Name.ToString();
    }

    public bool Equals(UsingDirectiveMeta other)
    {
        return UseVContainer == other.UseVContainer && AliasName == other.AliasName;
    }

    public override bool Equals(object? obj)
    {
        return obj is UsingDirectiveMeta other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (UseVContainer.GetHashCode() * 397) ^ (AliasName != null ? AliasName.GetHashCode() : 0);
        }
    }
}