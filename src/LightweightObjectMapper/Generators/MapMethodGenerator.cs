using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LightweightObjectMapper.Models;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Generators;

/// <summary>
/// 映射方法生成器抽象类
/// </summary>
internal abstract class MapMethodGenerator
{
    #region Public 属性

    public BuildContext Context { get; }

    public IdentifierMatcher IdentifierMatcher { get; }

    public TypeConversionMatcher TypeConversionMatcher { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="MapMethodGenerator"/>
    public MapMethodGenerator(BuildContext buildContext, IdentifierMatcher identifierMatcher, TypeConversionMatcher typeConversionMatcher)
    {
        Context = buildContext ?? throw new ArgumentNullException(nameof(buildContext));
        IdentifierMatcher = identifierMatcher ?? throw new ArgumentNullException(nameof(identifierMatcher));
        TypeConversionMatcher = typeConversionMatcher ?? throw new ArgumentNullException(nameof(typeConversionMatcher));
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <summary>
    /// 生成字段和属性的映射代码
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="descriptor"></param>
    /// <param name="sourceMeta"></param>
    /// <param name="targetMeta"></param>
    /// <param name="sourceAccessBody"></param>
    /// <param name="targetAccessBody"></param>
    /// <param name="lineEndMark"></param>
    /// <param name="includeInitOnly">包含init属性</param>
    /// <param name="ignoreIdentifiers">忽略的字段</param>
    /// <returns>无法找到匹配的标识符</returns>
    protected void GenerateFieldAndPropertyMapCode(StringBuilder builder,
                                                   TypeMapDescriptor descriptor,
                                                   TypeMapMetaData sourceMeta,
                                                   TypeMapMetaData targetMeta,
                                                   string sourceAccessBody,
                                                   string targetAccessBody,
                                                   string lineEndMark,
                                                   bool includeInitOnly = false,
                                                   IEnumerable<string>? ignoreIdentifiers = null)
    {
        List<TypeReadWriteIdentifier> notfoundIdentifiers = new();

        var targetIdentifiers = includeInitOnly
                                ? targetMeta.WriteableIdentifiers.Concat(targetMeta.InitOnlyIdentifiers).SelectMany(m => m.Value)
                                : targetMeta.WriteableIdentifiers.SelectMany(m => m.Value);

        foreach (var targetIdentifier in targetIdentifiers)  //处理可写标识符
        {
            if (ignoreIdentifiers?.Contains(targetIdentifier.Identifier) == true)
            {
                continue;
            }
            if (IdentifierMatcher.Match(targetIdentifier, sourceMeta.ReadableIdentifiers) is TypeIdentifierConversion conversion)
            {
                if (conversion)
                {
                    builder.AppendLine($"{targetAccessBody}{targetIdentifier.Identifier} = {conversion.Conversion.GetConversionExpression($"{sourceAccessBody}{conversion.Identifier!.Identifier}")}{lineEndMark}");
                    continue;
                }
                else if (!Context.TryReportTypeDiagnostic(descriptor.Nodes, conversion.ReportDiagnosticDescriptor, conversion.DiagnosticMessageArgs))
                {
                    notfoundIdentifiers.Add(targetIdentifier);
                    continue;
                }
            }
        }

        if (targetMeta.ReadonlyIdentifiers.Count > 0)   //只读字段无法映射
        {
            var readOnlyIdentifiers = sourceMeta.ReadableIdentifiers.Values.SelectMany(m => m)
                                                                           .Select(m => IdentifierMatcher.Match(m, targetMeta.ReadonlyIdentifiers))
                                                                           .Where(m => m)
                                                                           .ToArray();
            if (readOnlyIdentifiers.Length > 0)
            {
                Context.TryReportTypeDiagnostic(descriptor.Nodes, DiagnosticDescriptors.Warning.ReadOnlyFieldCanNotMap, new object[] { descriptor.TargetType, string.Join(", ", readOnlyIdentifiers.Select(m => m.Identifier!.Identifier)) });
            }
        }

        if (notfoundIdentifiers.Count > 0)
        {
            Context.TryReportTypeDiagnostic(descriptor.Nodes, DiagnosticDescriptors.Information.NotFoundMatchPropertyOrField, new[] { string.Join(", ", notfoundIdentifiers.Select(m => m.Identifier)) });
        }
    }

    /// <summary>
    /// 生成 PostMapping 的调用代码
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="sourceAccessBody"></param>
    /// <param name="targetAccessBody"></param>
    protected string? GeneratePostMappingInvokeCode(TypeMapDescriptor descriptor, string sourceAccessBody, string targetAccessBody)
    {
        if (Context.MappingProfiles.TryGetValue(new(descriptor.SourceType, descriptor.TargetType), out var profileItem)
            && profileItem.HasProfile
            && profileItem.HasPostMapping)
        {
            return profileItem.PostMappingInvokeDescriptor!.Value.FormatFullInvoke(sourceAccessBody, targetAccessBody);
        }
        return null;
    }

    /// <summary>
    /// 生成 TypeMapping 的调用代码
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="sourceAccessBody"></param>
    /// <param name="instanceAccessBody"></param>
    protected string? GenerateTypeMappingInvokeCode(TypeMapDescriptor descriptor, string sourceAccessBody, string instanceAccessBody)
    {
        if (Context.MappingProfiles.TryGetValue(new(descriptor.SourceType, descriptor.TargetType), out var profileItem)
            && profileItem.HasProfile
            && profileItem.HasTypeMapping)
        {
            return profileItem.TypeMappingInvokeDescriptor!.Value.FormatFullInvoke(sourceAccessBody, instanceAccessBody);
        }
        return null;
    }

    #endregion Protected 方法
}
