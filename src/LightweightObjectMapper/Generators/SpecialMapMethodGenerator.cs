using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LightweightObjectMapper.Models;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Generators;

/// <summary>
/// 特定对象映射方法体生成器
/// </summary>
internal class SpecialMapMethodGenerator : MapMethodGenerator
{
    #region Public 构造函数

    public SpecialMapMethodGenerator(BuildContext buildContext, IdentifierMatcher identifierMatcher, TypeConversionMatcher typeConversionMatcher) : base(buildContext, identifierMatcher, typeConversionMatcher)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public IEnumerable<string> Generate(TypeMapMetaData sourceMeta, IEnumerable<TypeMapDescriptor> mapDescriptors)
    {
        var builder = new StringBuilder(1024);

        foreach (var mapDescriptor in mapDescriptors)
        {
            try
            {
                var targetMeta = Context.GetTypeMapMetaData(mapDescriptor.TargetType);

                if (Context.TryReportTypeDiagnostic(mapDescriptor.Nodes, targetMeta.ReportDiagnosticDescriptor, targetMeta.DiagnosticMessageArgs))
                {
                    continue;
                }

                if (mapDescriptor is CollectionTypeMapDescriptor
                    && !mapDescriptor.WithOutTargetInstance)    //使用实例进行集合映射，报告错误
                {
                    Context.TryReportTypeDiagnostic(mapDescriptor.Nodes, DiagnosticDescriptors.Error.CollectionMapIncorrectUsage, new object[] { targetMeta.TypeSymbol.ToDisplayString() });
                    continue;
                }

                builder.Clear();
                GenerateMapMethodBody(builder, mapDescriptor, sourceMeta, targetMeta);
            }
            catch (Exception ex)
            {
                Context.ReportUnexpectedExceptionDiagnostic(ex);
                continue;
            }
            yield return builder.ToString();
        }
    }

    public void GenerateMapMethodBody(StringBuilder builder, TypeMapDescriptor descriptor, TypeMapMetaData sourceMeta, TypeMapMetaData targetMeta)
    {
        var sourceTypeName = sourceMeta.TypeSymbol.ToFullCodeString();
        var targetTypeName = targetMeta.TypeSymbol.ToFullCodeString();
        builder.AppendLine($@"/// <summary>
/// Map <see cref=""{sourceMeta.TypeSymbol.ToRemarkCodeString()}""/> to <see cref=""{targetMeta.TypeSymbol.ToRemarkCodeString()}""/>
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static {targetTypeName} {Constants.MapMethodName}(this {sourceTypeName} source,{targetMeta.GetParameterRefKeyword()} {targetTypeName} target)
{{");

        if (sourceMeta.IsNullableType)
        {
            builder.AppendLine(@"if (source == null)
{
    throw new ArgumentNullException(nameof(source));
}");
        }

        if (targetMeta.IsNullableType)
        {
            builder.AppendLine(@"if (target == null)
{
    throw new ArgumentNullException(nameof(target));
}");
        }

        GenerateFieldAndPropertyMapCode(builder, descriptor, sourceMeta, targetMeta, "source.", "target.", ";", includeInitOnly: false, ignoreIdentifiers: targetMeta.IgnoreMemberNames);

        if (GeneratePostMappingInvokeCode(descriptor, "source", "target") is string postMappingInvokeCode
            && !string.IsNullOrWhiteSpace(postMappingInvokeCode))
        {
            builder.AppendLine($"target = {postMappingInvokeCode};");
        }

        if (targetMeta.InitOnlyIdentifiers.Count > 0)   //init 属性无法映射
        {
            var initOnlyIdentifiers = sourceMeta.ReadableIdentifiers.Values.SelectMany(m => m)
                                                                           .Select(m => IdentifierMatcher.Match(m, targetMeta.InitOnlyIdentifiers))
                                                                           .Where(m => m)
                                                                           .ToArray();
            if (initOnlyIdentifiers.Length > 0)
            {
                Context.TryReportTypeDiagnostic(descriptor.Nodes, DiagnosticDescriptors.Warning.InitOnlyPropertyCanNotMap, new object[] { descriptor.TargetType, string.Join(", ", initOnlyIdentifiers.Select(m => m.Identifier!.Identifier)) });
            }
        }

        builder.AppendLine("return target;");

        builder.Append("}");
    }

    #endregion Public 方法
}
