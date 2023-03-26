using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

[DebuggerDisplay("{From} -> {To}")]
internal struct MappingPair : IEquatable<MappingPair>
{
    #region Public 属性

    public ITypeSymbol From { get; }

    public ITypeSymbol To { get; }

    #endregion Public 属性

    #region Public 构造函数

    public MappingPair(ITypeSymbol from, ITypeSymbol to)
    {
        From = from;
        To = to;
    }

    #endregion Public 构造函数

    #region Public 方法

    public bool Equals(MappingPair other)
    {
        return From.EqualsDefault(other.From) && To.EqualsDefault(other.To);
    }

    public override int GetHashCode()
    {
        int hashCode = -1781160927;
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(From);
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(To);
        return hashCode;
    }

    #endregion Public 方法
}
