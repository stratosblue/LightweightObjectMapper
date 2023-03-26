using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型映射描述
/// </summary>
[DebuggerDisplay("{SourceType} -> {TargetType} {Nodes.Count}")]
internal class TypeMapDescriptor : IEquatable<TypeMapDescriptor>
{
    #region Public 属性

    public Compilation? Compilation { get; set; }

    /// <summary>
    /// 原始语法节点列表
    /// </summary>
    public ImmutableList<SyntaxNode> Nodes { get; protected set; } = ImmutableList.Create<SyntaxNode>();

    /// <summary>
    /// 源类型
    /// </summary>
    public ITypeSymbol SourceType { get; protected set; }

    /// <summary>
    /// 目标类型
    /// </summary>
    public ITypeSymbol TargetType { get; protected set; }

    /// <summary>
    /// 在没有目标类型实例的情况下进行映射
    /// </summary>
    public bool WithOutTargetInstance { get; protected set; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeMapDescriptor(ITypeSymbol sourceType, ITypeSymbol targetType, bool withOutTargetInstance)
    {
        SourceType = sourceType;
        TargetType = targetType;
        WithOutTargetInstance = withOutTargetInstance;
    }

    #endregion Public 构造函数

    #region Public 方法

    public TypeMapDescriptor AddNode(SyntaxNode node)
    {
        Nodes = Nodes.Add(node);
        return this;
    }

    public TypeMapDescriptor AddNodes(IEnumerable<SyntaxNode> nodes)
    {
        Nodes = Nodes.AddRange(nodes);
        return this;
    }

    public virtual bool Equals(TypeMapDescriptor other)
    {
        if (other is null)
        {
            return false;
        }
        return WithOutTargetInstance == other.WithOutTargetInstance
               && GetType() == other.GetType()
               && SourceType.EqualsDefault(other.SourceType)
               && TargetType.EqualsDefault(other.TargetType);
    }

    public override int GetHashCode()
    {
        int hashCode = -1827821071;
        hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(SourceType);
        hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(TargetType);
        hashCode = hashCode * -1521134295 + WithOutTargetInstance.GetHashCode();
        return hashCode;
    }

    #endregion Public 方法
}

/// <summary>
/// 集合类型映射描述
/// </summary>
[DebuggerDisplay("[Collection]{SourceItemType} -> {TargetItemType} {Nodes.Count}")]
internal class CollectionTypeMapDescriptor : TypeMapDescriptor
{
    #region Public 属性

    /// <summary>
    /// 源元素类型
    /// </summary>
    public ITypeSymbol SourceItemType { get; protected set; }

    /// <summary>
    /// 目标元素类型
    /// </summary>
    public ITypeSymbol TargetItemType { get; protected set; }

    #endregion Public 属性

    #region Public 构造函数

    public CollectionTypeMapDescriptor(ITypeSymbol sourceType, ITypeSymbol targetType, ITypeSymbol sourceItemType, ITypeSymbol targetItemType, bool withOutTargetInstance)
        : base(sourceType, targetType, withOutTargetInstance)
    {
        SourceItemType = sourceItemType;
        TargetItemType = targetItemType;
    }

    #endregion Public 构造函数

    #region Public 方法

    public static bool TryCreate(Compilation compilation, ITypeSymbol sourceType, ITypeSymbol targetType, bool withOutTargetInstance, out CollectionTypeMapDescriptor? collectionTypeMapDescriptor)
    {
        if (compilation is null)
        {
            throw new ArgumentNullException(nameof(compilation));
        }

        var iEnumerableT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
        if (WellknownTypeCollection.TryGetIEnumerableItemGenericType(sourceType, iEnumerableT, out var iEnumerableInterfaceTypeSymbolSource)
               && WellknownTypeCollection.TryGetIEnumerableItemGenericType(targetType, iEnumerableT, out var iEnumerableInterfaceTypeSymbolTarget)
               && iEnumerableInterfaceTypeSymbolSource is not null
               && iEnumerableInterfaceTypeSymbolTarget is not null)
        {
            collectionTypeMapDescriptor = new CollectionTypeMapDescriptor(sourceType,
                                                                          targetType,
                                                                          iEnumerableInterfaceTypeSymbolSource.TypeArguments.First(),
                                                                          iEnumerableInterfaceTypeSymbolTarget.TypeArguments.First(),
                                                                          withOutTargetInstance);
            return true;
        }
        collectionTypeMapDescriptor = null;
        return false;
    }

    public override bool Equals(TypeMapDescriptor other)
    {
        if (other is null)
        {
            return false;
        }
        if (other is CollectionTypeMapDescriptor otherCollectionTypeMapDescriptor)
        {
            return WithOutTargetInstance == otherCollectionTypeMapDescriptor.WithOutTargetInstance
                   && SourceItemType.EqualsDefault(otherCollectionTypeMapDescriptor.SourceItemType)
                   && TargetType.EqualsDefault(otherCollectionTypeMapDescriptor.TargetType);
        }
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        int hashCode = -1827821071;
        hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(SourceItemType);
        hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(TargetType);
        hashCode = hashCode * -1521134295 + WithOutTargetInstance.GetHashCode();
        return hashCode;
    }

    #endregion Public 方法
}
