using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

/// <summary>
/// mapper自己的类型集合
/// </summary>
public class SelfTypeCollection
{
    #region Public 属性

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.CollectionMappingAttribute"/>
    /// <see cref="LightweightObjectMapper.CollectionMappingAttribute"/>
    /// </summary>
    public INamedTypeSymbol CollectionMappingAttribute { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.IMappingPrepare{TIn, TOut}"/>
    /// <see cref="LightweightObjectMapper.IMappingPrepare{TIn, TOut}"/>
    /// </summary>
    public ITypeSymbol IMappingPrepare { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.IMappingProfile"/>
    /// <see cref="LightweightObjectMapper.IMappingProfile"/>
    /// </summary>
    public ITypeSymbol IMappingProfile { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.IPostMapping{TIn, TOut}"/>
    /// <see cref="LightweightObjectMapper.IPostMapping{TIn, TOut}"/>
    /// </summary>
    public ITypeSymbol IPostMapping { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.ITypeMapping{TIn, TOut}"/>
    /// <see cref="LightweightObjectMapper.ITypeMapping{TIn, TOut}"/>
    /// </summary>
    public ITypeSymbol ITypeMapping { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.ITypeMemberIgnoreMapping{T}"/>
    /// <see cref="LightweightObjectMapper.ITypeMemberIgnoreMapping{T}"/>
    /// </summary>
    public ITypeSymbol ITypeMemberIgnoreMapping { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.MappingMetadataAttribute"/>
    /// <see cref="LightweightObjectMapper.MappingMetadataAttribute"/>
    /// </summary>
    public INamedTypeSymbol MappingMetadataAttribute { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.MappingProfileIncludeAttribute"/>
    /// <see cref="LightweightObjectMapper.MappingProfileIncludeAttribute"/>
    /// </summary>
    public INamedTypeSymbol MappingProfileIncludeAttribute { get; }

    /// <summary>
    /// <inheritdoc cref="LightweightObjectMapper.PredefinedSpecialTypeMapping"/>
    /// <see cref="LightweightObjectMapper.PredefinedSpecialTypeMapping"/>
    /// </summary>
    public ITypeSymbol PredefinedSpecialTypeMapping { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="SelfTypeCollection"/>
    public SelfTypeCollection(Compilation compilation)
    {
        PredefinedSpecialTypeMapping = compilation.GetTypeByMetadataName($"LightweightObjectMapper.{nameof(LightweightObjectMapper.PredefinedSpecialTypeMapping)}")!;
        IMappingProfile = compilation.GetTypeByMetadataName($"LightweightObjectMapper.{nameof(LightweightObjectMapper.IMappingProfile)}")!;
        IMappingPrepare = compilation.GetTypeByMetadataName("LightweightObjectMapper.IMappingPrepare`2")!;
        ITypeMapping = compilation.GetTypeByMetadataName("LightweightObjectMapper.ITypeMapping`2")!;
        IPostMapping = compilation.GetTypeByMetadataName("LightweightObjectMapper.IPostMapping`2")!;
        ITypeMemberIgnoreMapping = compilation.GetTypeByMetadataName($"LightweightObjectMapper.ITypeMemberIgnoreMapping`1")!;
        MappingProfileIncludeAttribute = compilation.GetTypeByMetadataName("LightweightObjectMapper.MappingProfileIncludeAttribute")!;
        CollectionMappingAttribute = compilation.GetTypeByMetadataName($"LightweightObjectMapper.{nameof(LightweightObjectMapper.CollectionMappingAttribute)}")!;
        MappingMetadataAttribute = compilation.GetTypeByMetadataName($"LightweightObjectMapper.{nameof(LightweightObjectMapper.MappingMetadataAttribute)}")!;
    }

    #endregion Public 构造函数
}
