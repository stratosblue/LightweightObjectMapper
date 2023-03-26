using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 构造函数匹配结果
/// </summary>
[DebuggerDisplay("{IsSuccess} {Constructor}")]
internal struct ConstructorMatchResult
{
    #region Public 属性

    public TypeConstructorDescriptor? Constructor { get; }

    public bool IsSuccess { get; }

    public ImmutableArray<TypeIdentifierConversion>? OrderedParameterConversions { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ConstructorMatchResult(bool isSuccess, TypeConstructorDescriptor? constructor, ImmutableArray<TypeIdentifierConversion>? orderedParameterConversions)
    {
        IsSuccess = isSuccess;
        if (isSuccess)
        {
            if (constructor is null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }
            if (orderedParameterConversions is null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }
        }
        Constructor = constructor;
        OrderedParameterConversions = orderedParameterConversions;
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator bool(ConstructorMatchResult value)
    {
        return value.IsSuccess;
    }

    #endregion Public 方法
}
