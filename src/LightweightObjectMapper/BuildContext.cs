using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using LightweightObjectMapper.Models;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

/// <summary>
/// 构造上下文
/// </summary>
internal class BuildContext
{
    #region Public 属性

    public CancellationToken CancellationToken => Context.CancellationToken;

    /// <summary>
    /// 集合映射配置
    /// </summary>
    public ConcurrentDictionary<ITypeSymbol, CollectionMappingProfileItem> CollectionMappingProfiles { get; } = new(SymbolEqualityComparer.Default);

    public Compilation Compilation { get; }

    /// <summary>
    /// 编译属性
    /// </summary>
    public CompilationProperties CompilationProperties { get; }

    /// <summary>
    /// 生成器执行上下文
    /// </summary>
    public SourceProductionContext Context { get; }

    /// <summary>
    /// 映射配置
    /// </summary>
    public ConcurrentDictionary<MappingPair, MappingProfileItem> MappingProfiles { get; } = new();

    /// <summary>
    /// 支持 Nullable
    /// </summary>
    public bool NullableSupported { get; set; }

    /// <summary>
    /// Mapper自己定义的类型
    /// </summary>
    public SelfTypeCollection SelfTypes { get; }

    /// <summary>
    /// 类型的忽略映射元数据字典
    /// </summary>
    public ConcurrentDictionary<ITypeSymbol, TypeMemberIgnoreMappingMeta> TypeIgnoreMemberMetas { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 类型映射描述
    /// </summary>
    public ConcurrentDictionary<TypeMapDescriptor, TypeMapDescriptor> TypeMapDescriptors { get; } = new();

#pragma warning disable RS1024 // 正确比较符号

    /// <summary>
    /// 类型映射元数据字典
    /// </summary>
    public ConcurrentDictionary<ITypeSymbol, TypeMapMetaData> TypeMapMetaDatas { get; } = new(SymbolEqualityComparer.Default);

#pragma warning restore RS1024 // 正确比较符号

    /// <summary>
    /// 不支持的类型符号集合
    /// </summary>
    public ImmutableHashSet<ITypeSymbol> UnsupportedNamedTypeSymbols { get; }

    /// <summary>
    /// 已知类型
    /// </summary>
    public WellknownTypeCollection WellknownTypes { get; }

    #endregion Public 属性

    #region Public 构造函数

    public BuildContext(SourceProductionContext executionContext, Compilation compilation, CompilationProperties compilationProperties)
    {
        Context = executionContext;
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        CompilationProperties = compilationProperties ?? throw new ArgumentNullException(nameof(compilationProperties));

        UnsupportedNamedTypeSymbols = ImmutableHashSet.Create<ITypeSymbol>(
            SymbolEqualityComparer.Default,
            compilation.GetTypeByMetadataName("System.Boolean")!,
            compilation.GetTypeByMetadataName("System.Byte")!,
            compilation.GetTypeByMetadataName("System.SByte")!,
            compilation.GetTypeByMetadataName("System.Int16")!,
            compilation.GetTypeByMetadataName("System.UInt16")!,
            compilation.GetTypeByMetadataName("System.Int32")!,
            compilation.GetTypeByMetadataName("System.UInt32")!,
            compilation.GetTypeByMetadataName("System.Int64")!,
            compilation.GetTypeByMetadataName("System.UInt64")!,
            compilation.GetTypeByMetadataName("System.IntPtr")!,
            compilation.GetTypeByMetadataName("System.UIntPtr")!,
            compilation.GetTypeByMetadataName("System.Single")!,
            compilation.GetTypeByMetadataName("System.Double")!,
            compilation.GetTypeByMetadataName("System.Decimal")!,
            compilation.GetTypeByMetadataName("System.TimeSpan")!,
            compilation.GetTypeByMetadataName("System.DateTime")!,
            compilation.GetTypeByMetadataName("System.Guid")!,
            compilation.GetTypeByMetadataName("System.Char")!,
            compilation.GetTypeByMetadataName("System.String")!,
            compilation.GetSpecialType(SpecialType.System_IDisposable),
            compilation.GetSpecialType(SpecialType.System_Object),
            compilation.GetSpecialType(SpecialType.System_Void)
            );
        WellknownTypes = new(compilation);
        SelfTypes = new(compilation);

        NullableSupported = compilation.Options.NullableContextOptions != NullableContextOptions.Disable;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 获取类型的映射元数据
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public TypeMapMetaData GetTypeMapMetaData(ITypeSymbol typeSymbol)
    {
        if (TypeMapMetaDatas.TryGetValue(typeSymbol, out var metaData))
        {
            return metaData;
        }
        metaData = TypeMapMetaData.Create(this, typeSymbol);
        TypeMapMetaDatas.TryAdd(typeSymbol, metaData);
        return metaData;
    }

    /// <summary>
    /// 检查类型是否不受支持
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public bool IsUnsupportedType(ITypeSymbol typeSymbol)
    {
        return UnsupportedNamedTypeSymbols.Contains(typeSymbol);
    }

    /// <summary>
    /// 报告构建中出现意外的异常
    /// </summary>
    /// <param name="exception"></param>
    public void ReportUnexpectedExceptionDiagnostic(Exception exception)
    {
        Context.ReportDiagnostic(Diagnostic.Create(descriptor: DiagnosticDescriptors.Error.UnexpectedExceptionWhileBuilding, location: null, messageArgs: exception));
    }

    /// <summary>
    /// 尝试报告代码诊断，并返回是否进行了报告
    /// </summary>
    /// <param name="diagnosticDescriptor"></param>
    /// <param name="messageArgs"></param>
    /// <returns></returns>
    public bool TryReportTypeDiagnostic(DiagnosticDescriptor? diagnosticDescriptor, object?[]? messageArgs)
    {
        if (diagnosticDescriptor is null)
        {
            return false;
        }

        Context.ReportDiagnostic(Diagnostic.Create(descriptor: diagnosticDescriptor, location: null, messageArgs: messageArgs));
        return true;
    }

    /// <summary>
    /// 尝试报告代码诊断，并返回是否进行了报告
    /// </summary>
    /// <param name="node"></param>
    /// <param name="diagnosticDescriptor"></param>
    /// <param name="messageArgs"></param>
    /// <returns></returns>
    public bool TryReportTypeDiagnostic(SyntaxNode node, DiagnosticDescriptor? diagnosticDescriptor, object?[]? messageArgs)
    {
        if (diagnosticDescriptor is null)
        {
            return false;
        }

        Context.ReportDiagnostic(Diagnostic.Create(descriptor: diagnosticDescriptor, location: node.GetLocation(), messageArgs: messageArgs));
        return true;
    }

    /// <summary>
    /// 尝试报告代码诊断，并返回是否进行了报告
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="diagnosticDescriptor"></param>
    /// <param name="messageArgs"></param>
    /// <returns></returns>
    public bool TryReportTypeDiagnostic(IEnumerable<SyntaxNode>? nodes, DiagnosticDescriptor? diagnosticDescriptor, object?[]? messageArgs)
    {
        if (diagnosticDescriptor is null)
        {
            return false;
        }

        Context.ReportDiagnostic(Diagnostic.Create(descriptor: diagnosticDescriptor, location: nodes?.FirstOrDefault()?.GetLocation(), additionalLocations: nodes?.Skip(1).Select(m => m.GetLocation()), messageArgs: messageArgs));
        return true;
    }

    #endregion Public 方法
}
