using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 加载的配置方法元数据
/// </summary>
internal class ProfileLoadMethodMeta
{
    #region Public 属性

    /// <summary>
    /// Prepare时为需要忽略的成员列表
    /// </summary>
    public string[] Fields { get; }

    public AttributeData[] MappingMetadatas { get; }

    public MappingMetadataType MetadataType { get; }

    public IMethodSymbol MethodSymbol { get; }

    public ITypeSymbol[] TypeSymbols { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ProfileLoadMethodMeta(MappingMetadataType metadataType, IMethodSymbol methodSymbol, AttributeData[] mappingMetadatas, string[] fields, ITypeSymbol[] typeSymbols)
    {
        MetadataType = metadataType;
        MethodSymbol = methodSymbol;
        MappingMetadatas = mappingMetadatas;
        Fields = fields;
        TypeSymbols = typeSymbols;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override bool Equals(object? obj)
    {
        return obj is ProfileLoadMethodMeta other &&
               MetadataType == other.MetadataType &&
               EqualityComparer<IMethodSymbol>.Default.Equals(MethodSymbol, other.MethodSymbol) &&
               EqualityComparer<AttributeData[]>.Default.Equals(MappingMetadatas, other.MappingMetadatas) &&
               EqualityComparer<string[]>.Default.Equals(Fields, other.Fields) &&
               EqualityComparer<ITypeSymbol[]>.Default.Equals(TypeSymbols, other.TypeSymbols);
    }

    public override int GetHashCode()
    {
        int hashCode = 1716640747;
        hashCode = hashCode * -1521134295 + MetadataType.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<IMethodSymbol>.Default.GetHashCode(MethodSymbol);
        hashCode = hashCode * -1521134295 + EqualityComparer<AttributeData[]>.Default.GetHashCode(MappingMetadatas);
        hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Fields);
        hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol[]>.Default.GetHashCode(TypeSymbols);
        return hashCode;
    }

    #endregion Public 方法
}
