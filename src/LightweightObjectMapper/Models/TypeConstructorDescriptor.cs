using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型构造函数描述
/// </summary>
[DebuggerDisplay("{Method}({OrderedParameters.Length})")]
internal class TypeConstructorDescriptor
{
    #region Public 属性

    /// <summary>
    /// 构造方法
    /// </summary>
    public IMethodSymbol Method { get; }

    /// <summary>
    /// 有序的参数列表
    /// </summary>
    public ImmutableArray<TypeReadWriteIdentifier> OrderedParameters { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeConstructorDescriptor(IMethodSymbol method)
    {
        Method = method ?? throw new ArgumentNullException(nameof(method));
        OrderedParameters = Method.Parameters.Select(m => new TypeReadWriteIdentifier(m.Name, m, kind: m.Kind, typeSymbol: m.Type, readable: false, writeable: true))
                                             .ToImmutableArray();
    }

    #endregion Public 构造函数
}
