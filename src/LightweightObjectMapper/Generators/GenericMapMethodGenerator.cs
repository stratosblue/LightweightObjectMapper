using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LightweightObjectMapper.Models;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Generators;

/// <summary>
/// 泛型映射方法体生成器
/// </summary>
internal class GenericMapMethodGenerator : MapMethodGenerator
{
    #region Public 构造函数

    public GenericMapMethodGenerator(BuildContext buildContext, IdentifierMatcher identifierMatcher, TypeConversionMatcher typeConversionMatcher) : base(buildContext, identifierMatcher, typeConversionMatcher)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public string? Generate(TypeMapMetaData sourceMeta, IEnumerable<TypeMapDescriptor> mapDescriptors)
    {
        if (mapDescriptors.Any())
        {
            try
            {
                var builder = new StringBuilder(1024);
                GenerateMapMethodBody(builder, sourceMeta, mapDescriptors);
                return builder.ToString();
            }
            catch (Exception ex)
            {
                Context.ReportUnexpectedExceptionDiagnostic(ex);
            }
        }
        return null;
    }

    public void GenerateMapMethodBody(StringBuilder builder, TypeMapMetaData sourceMeta, IEnumerable<TypeMapDescriptor> targetDescriptors)
    {
        var sourceTypeName = sourceMeta.TypeSymbol.ToFullCodeString();
        builder.AppendLine($@"/// <summary>
/// Map <see cref=""{sourceMeta.TypeSymbol.ToRemarkCodeString()}""/> to the following types:<br/>");

        foreach (var targetTypeSymbol in targetDescriptors.Select(m => m.TargetType))
        {
            if (targetTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol)
            {
                builder.AppendLine($"/// <see cref=\"{targetTypeSymbol.ToRemarkCodeString()}\"/><br/>");
            }
            else
            {
                builder.AppendLine($"/// <see cref=\"{arrayTypeSymbol.ElementType.ToRemarkCodeString()}\"/>[]<br/>");
            }
        }

        builder.AppendLine($@"/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static TOut {Constants.MapMethodName}<TOut>(this {sourceTypeName} source)");

        if (sourceMeta.IsNullableType)
        {
            builder.AppendLine(@"{
if (source == null)
{
    throw new ArgumentNullException(nameof(source));
}");
        }
        else
        {
            builder.AppendLine("{");
        }
        var lastGenerateResult = false;
        foreach (var targetDescriptor in targetDescriptors)
        {
            if (targetDescriptor is CollectionTypeMapDescriptor collectionTypeMapDescriptor)
            {
                lastGenerateResult = GenerateCollectionTypeMapBlock(sourceMeta, collectionTypeMapDescriptor, builder, lastGenerateResult);
            }
            else
            {
                lastGenerateResult = GenerateSingleTypeMapBlock(sourceMeta, targetDescriptor, builder, lastGenerateResult);
            }
        }

        builder.AppendLine($"throw new global::System.NotImplementedException($\"No mapping code for {{typeof(TOut)}}.\");");

        builder.Append("}");
    }

    #endregion Public 方法

    #region Private 方法

    private static string BuildCtorExpression(ConstructorMatchResult constructorMatchResult, string targetTypeName)
    {
        var conversions = constructorMatchResult.OrderedParameterConversions!.Value;
        if (conversions.Length == 0)
        {
            return $"new {targetTypeName}()";
        }
        var parameters = constructorMatchResult.Constructor!.OrderedParameters;
        return $"new {targetTypeName}({string.Join(", ", conversions.Select((m, index) => $"{parameters[index].Identifier}: {m.Conversion.GetConversionExpression($"source.{m.Identifier!.Identifier}")}"))})";
    }

    private bool GenerateCollectionTypeMapBlock(TypeMapMetaData sourceMeta, CollectionTypeMapDescriptor targetDescriptor, StringBuilder builder, bool hasElse)
    {
        var conversion = TypeConversionMatcher.GetTypeConversion(targetDescriptor.SourceType, targetDescriptor.TargetType, false);
        if (!conversion)
        {
            Context.TryReportTypeDiagnostic(targetDescriptor.Nodes, DiagnosticDescriptors.Error.UndefinedCollectionMapping, new object[] { targetDescriptor.SourceType, targetDescriptor.TargetType });
            return false;
        }
        var postMappingInvokeCode = GeneratePostMappingInvokeCode(targetDescriptor, "source", "target");
        if (!string.IsNullOrWhiteSpace(postMappingInvokeCode))
        {
            postMappingInvokeCode = $"target = {postMappingInvokeCode};";
        }

        var targetTypeName = targetDescriptor.TargetType.ToFullCodeString();

        builder.AppendLine($@"{(hasElse ? "else " : string.Empty)}if (typeof(TOut) == typeof({targetTypeName}))
{{
var target = {conversion.GetConversionExpression("source")};
{postMappingInvokeCode}
return (TOut)(target as object);}}");

        return true;
    }

    private bool GenerateSingleTypeMapBlock(TypeMapMetaData sourceMeta, TypeMapDescriptor targetDescriptor, StringBuilder builder, bool hasElse)
    {
        var targetMeta = Context.GetTypeMapMetaData(targetDescriptor.TargetType);

        if (Context.TryReportTypeDiagnostic(targetDescriptor.Nodes, targetMeta.ReportDiagnosticDescriptor, targetMeta.DiagnosticMessageArgs))   //元数据有诊断报告
        {
            return false;
        }

        var prepareTypeConversion = TypeConversionMatcher.GetMappingPrepareTypeConversion(targetDescriptor.SourceType, targetDescriptor.TargetType);

        var constructorMatchResult = IdentifierMatcher.MatchConstructor(targetMeta.AvailableConstructors, sourceMeta.ReadableIdentifiers);

        if (!prepareTypeConversion
            && !constructorMatchResult) //没有 MappingPrepare , 没有找到构造函数
        {
            if (targetMeta.AvailableConstructors.Length == 0)   //没有公共构造函数
            {
                Context.TryReportTypeDiagnostic(targetDescriptor.Nodes, DiagnosticDescriptors.Error.NoAnyPublicConstructor, new object[] { targetDescriptor.TargetType });
            }
            else
            {
                Context.TryReportTypeDiagnostic(targetDescriptor.Nodes, DiagnosticDescriptors.Error.ConstructorMatchFailed, new object[] { targetDescriptor.TargetType });
            }
            return false;
        }

        var targetTypeName = targetMeta.TypeSymbol.ToFullCodeString();

        var postMappingInvokeCode = GeneratePostMappingInvokeCode(targetDescriptor, "source", "target");
        if (!string.IsNullOrWhiteSpace(postMappingInvokeCode))
        {
            postMappingInvokeCode = $"target = {postMappingInvokeCode};";
        }

        var bodyBuilder = new StringBuilder(1024);

        var elseBlock = hasElse ? "else " : string.Empty;

        if (GenerateTypeMappingInvokeCode(targetDescriptor, "source", "default") is string typeMappingInvokeCode)  //有 TypeMapping
        {
            builder.AppendLine($@"{elseBlock}if (typeof(TOut) == typeof({targetTypeName}))
{{
var target = {typeMappingInvokeCode};
{bodyBuilder}
{postMappingInvokeCode}
return (TOut)(target as object);
}}");
        }
        else if (prepareTypeConversion)   //有映射准备
        {
            var ignoreIdentifiers = prepareTypeConversion.ConversionInvokeDescriptor!.Value.States is null
                                    ? targetMeta.IgnoreMemberNames
                                    : targetMeta.IgnoreMemberNames.Concat(prepareTypeConversion.ConversionInvokeDescriptor.Value.States.OfType<string>());
            GenerateFieldAndPropertyMapCode(bodyBuilder, targetDescriptor, sourceMeta, targetMeta, "source.", "target.", ";", includeInitOnly: false, ignoreIdentifiers: ignoreIdentifiers);

            builder.AppendLine($@"{elseBlock}if (typeof(TOut) == typeof({targetTypeName}))
{{
var target = {prepareTypeConversion.GetConversionExpression("source")};
{bodyBuilder}
{postMappingInvokeCode}
return (TOut)(target as object);
}}");
        }
        else    //没有映射准备，手动构建对象
        {
            GenerateFieldAndPropertyMapCode(bodyBuilder, targetDescriptor, sourceMeta, targetMeta, "source.", string.Empty, ",", includeInitOnly: true, ignoreIdentifiers: targetMeta.IgnoreMemberNames);

            builder.AppendLine($@"{elseBlock}if (typeof(TOut) == typeof({targetTypeName}))
{{
var target = {BuildCtorExpression(constructorMatchResult, targetTypeName)}
{{
{bodyBuilder}
}};
{postMappingInvokeCode}
return (TOut)(target as object);
}}");
        }

        return true;
    }

    #endregion Private 方法
}
