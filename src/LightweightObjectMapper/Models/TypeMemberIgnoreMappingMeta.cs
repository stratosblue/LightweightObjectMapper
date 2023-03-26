using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型忽略映射成员元数据
/// </summary>
internal struct TypeMemberIgnoreMappingMeta : IEquatable<TypeMemberIgnoreMappingMeta>
{
    #region Public 属性

    public bool IsEmpty => TargetType is null || MemberNames.Count == 0;

    public HashSet<string> MemberNames { get; } = new HashSet<string>();

    public ITypeSymbol TargetType { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeMemberIgnoreMappingMeta()
    {
        TargetType = null!;
    }

    public TypeMemberIgnoreMappingMeta(ITypeSymbol targetType) : this()
    {
        TargetType = targetType;
    }

    public bool Equals(TypeMemberIgnoreMappingMeta other)
    {
        return TargetType.EqualsDefault(other.TargetType);
    }

    public override bool Equals(object obj)
    {
        return obj is TypeMemberIgnoreMappingMeta other
               && Equals(other);
    }

    public override int GetHashCode()
    {
        return SymbolEqualityComparer.Default.GetHashCode(TargetType);
    }

    #endregion Public 构造函数
}
