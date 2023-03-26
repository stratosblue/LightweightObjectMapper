using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型映射元数据
/// </summary>
[DebuggerDisplay("{TypeSymbol}")]
internal class TypeMapMetaData
{
    #region Public 属性

    /// <summary>
    /// 所有可用的构造函数
    /// </summary>
    public ImmutableArray<TypeConstructorDescriptor> AvailableConstructors { get; private set; }

    /// <summary>
    /// <see cref="ReportDiagnosticDescriptor"/> 的消息参数
    /// </summary>
    public object?[]? DiagnosticMessageArgs { get; private set; }

    /// <summary>
    /// 忽略的成员名称列表
    /// </summary>
    public ImmutableHashSet<string> IgnoreMemberNames { get; private set; }

    /// <summary>
    /// 只在初始化时可用的标识符
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> InitOnlyIdentifiers { get; private set; }

    /// <summary>
    /// 是否可进行映射
    /// </summary>
    public bool IsMappable { get; private set; } = true;

    /// <summary>
    /// 此类型是否可以为 <see langword="null"/>
    /// </summary>
    public bool IsNullableType { get; private set; }

    /// <summary>
    /// 是否是值类型
    /// </summary>
    public bool IsValueType => TypeSymbol.IsValueType;

    /// <summary>
    /// 类型的所有可读标识符
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> ReadableIdentifiers { get; private set; }

    /// <summary>
    /// 类型的所有只读标识符
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> ReadonlyIdentifiers { get; private set; }

    /// <summary>
    /// 用于报告IDE的信息
    /// </summary>
    public DiagnosticDescriptor? ReportDiagnosticDescriptor { get; private set; }

    /// <summary>
    /// 元数据对应的类型
    /// </summary>
    public ITypeSymbol TypeSymbol { get; }

    /// <summary>
    /// 类型的所有可写标识符
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> WriteableIdentifiers { get; private set; }

    #endregion Public 属性

    #region Private 构造函数

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    private TypeMapMetaData(ITypeSymbol typeSymbol)
    {
        TypeSymbol = typeSymbol;
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    #endregion Private 构造函数

    #region Public 方法

    public static TypeMapMetaData Create(BuildContext context, ITypeSymbol typeSymbol)
    {
        var metaData = new TypeMapMetaData(typeSymbol);
        if (context.IsUnsupportedType(typeSymbol))
        {
            metaData.IsMappable = false;
            metaData.ReportDiagnosticDescriptor = DiagnosticDescriptors.Error.AmbiguousMapType;
            metaData.DiagnosticMessageArgs = new[] { typeSymbol };
            return metaData;
        }

        var allMembers = typeSymbol.GetAllMembers();
        var typeMembers = typeSymbol.GetMembers();

        metaData.AvailableConstructors = typeMembers.OfType<IMethodSymbol>()
                                                    .Where(m => m.MethodKind == MethodKind.Constructor && m.DeclaredAccessibility == Accessibility.Public)
                                                    .OrderBy(m => m.Parameters.Length)
                                                    .Select(m => new TypeConstructorDescriptor(m))
                                                    .ToImmutableArray();
        var properties = allMembers.OfType<IPropertySymbol>()
                                   .Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsIndexer)
                                   .Select(m => new TypeReadWriteIdentifier(m.Name, m, kind: m.Kind, typeSymbol: m.Type, readable: !m.IsWriteOnly, writeable: !m.IsReadOnly))
                                   .ToImmutableArray();
        var fields = allMembers.OfType<IFieldSymbol>()
                               .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                               .Select(m => new TypeReadWriteIdentifier(m.Name, m, kind: m.Kind, typeSymbol: m.Type, readable: true, writeable: !m.IsReadOnly))
                               .ToImmutableArray();

        metaData.ReadableIdentifiers = properties.Where(m => m.Readable)
                                                 .Concat(fields.Where(m => m.Readable))
                                                 .GroupBy(m => m.Identifier.ToLowerInvariant())
                                                 .ToImmutableDictionary(m => m.Key, m => ImmutableArray.Create(m.OrderBy(m => m.Identifier).ToArray()), StringComparer.OrdinalIgnoreCase);

        metaData.ReadonlyIdentifiers = fields.Where(m => !m.Writeable)
                                             .GroupBy(m => m.Identifier.ToLowerInvariant())
                                             .ToImmutableDictionary(m => m.Key, m => ImmutableArray.Create(m.OrderBy(m => m.Identifier).ToArray()), StringComparer.OrdinalIgnoreCase);

        metaData.WriteableIdentifiers = properties.Where(m => m.Writeable && !m.IsInitOnly)
                                                  .Concat(fields.Where(m => m.Writeable))
                                                  .GroupBy(m => m.Identifier.ToLowerInvariant())
                                                  .ToImmutableDictionary(m => m.Key, m => ImmutableArray.Create(m.OrderBy(m => m.Identifier).ToArray()), StringComparer.OrdinalIgnoreCase);

        metaData.InitOnlyIdentifiers = properties.Where(m => m.Writeable && m.IsInitOnly)
                                                 .GroupBy(m => m.Identifier.ToLowerInvariant())
                                                 .ToImmutableDictionary(m => m.Key, m => ImmutableArray.Create(m.OrderBy(m => m.Identifier).ToArray()), StringComparer.OrdinalIgnoreCase);

        metaData.IgnoreMemberNames = context.TypeIgnoreMemberMetas.TryGetValue(typeSymbol, out var ignoreMappingMeta)
                                     ? ignoreMappingMeta.MemberNames.ToImmutableHashSet()
                                     : ImmutableHashSet<string>.Empty;

        metaData.IsNullableType = !typeSymbol.IsValueType
                                  || typeSymbol.OriginalDefinition.EqualsDefault(context.WellknownTypes.NullableT);

        return metaData;
    }

    #endregion Public 方法
}
