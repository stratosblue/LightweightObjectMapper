using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using LightweightObjectMapper.Models;
using LightweightObjectMapper.Util;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Generators;

internal class MapExtensionMethodGenerator
{
    #region Private 字段

    private readonly GenericMapMethodGenerator _genericMapMethodGenerator;
    private readonly SpecialMapMethodGenerator _specialMapMethodGenerator;

    #endregion Private 字段

    #region Public 属性

    public BuildContext Context { get; }

    public IdentifierMatcher IdentifierMatcher { get; }

    public ImmutableArray<TypeMapDescriptorGroup> MapDescriptorGroups { get; }

    public TypeConversionMatcher TypeConversionMatcher { get; }

    #endregion Public 属性

    #region Public 构造函数

    public MapExtensionMethodGenerator(BuildContext buildContext)
    {
        Context = buildContext ?? throw new ArgumentNullException(nameof(buildContext));
        TypeConversionMatcher = new(buildContext);
        IdentifierMatcher = new(TypeConversionMatcher);
        MapDescriptorGroups = buildContext.TypeMapDescriptors.Values.GroupBy(NormalizationSourceType).Select(m => new TypeMapDescriptorGroup(m.Key, m)).ToImmutableArray();
        _genericMapMethodGenerator = new(buildContext, IdentifierMatcher, TypeConversionMatcher);
        _specialMapMethodGenerator = new(buildContext, IdentifierMatcher, TypeConversionMatcher);

        ITypeSymbol NormalizationSourceType(TypeMapDescriptor typeMapDescriptor)
        {
            if (Context.WellknownTypes.TryGetIEnumerableItemGenericType(typeMapDescriptor.SourceType, out var iEnumerableInterfaceTypeSymbol)
                && iEnumerableInterfaceTypeSymbol is not null)
            {
                return iEnumerableInterfaceTypeSymbol;
            }
            else
            {
                return typeMapDescriptor.SourceType;
            }
        }
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Generate()
    {
        var className = $"LightweightObjectMapperMapExtensions_{Context.Compilation.Assembly.Name.Replace(".", "_")}";

        var accessibility = Context.CompilationProperties.MappingMethodAccessibility == Accessibility.Public
                            ? "public"
                            : "internal";

        var codeBuilder = new StringBuilder(@$"{Constants.CodeFileHeader}

using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace {Constants.RootNamespace}
{{
    {accessibility} static class {className}
    {{
", 4096);

        foreach (var methodCode in GenerateMethods().Where(m => !string.IsNullOrWhiteSpace(m)))
        {
            codeBuilder.AppendLine($"{methodCode}{Environment.NewLine}");
        }
        codeBuilder.AppendLine($"}}{Environment.NewLine}}}");

        Context.Context.AddSource(className, CodeFormatUtil.Format(codeBuilder.ToString(), Context.CancellationToken));
    }

    #endregion Public 方法

    #region Private 方法

    private IEnumerable<string?> GenerateMethods()
    {
        foreach (var group in MapDescriptorGroups)
        {
            var sourceMeta = Context.GetTypeMapMetaData(group.SourceType);

            if (Context.TryReportTypeDiagnostic(group.Descriptors.SelectMany(m => m.Nodes), sourceMeta.ReportDiagnosticDescriptor, sourceMeta.DiagnosticMessageArgs))
            {
                continue;
            }

            var genericMapDescriptors = group.Descriptors.Where(m => m.WithOutTargetInstance).ToArray();

            yield return _genericMapMethodGenerator.Generate(sourceMeta, genericMapDescriptors);

            foreach (var methodBody in _specialMapMethodGenerator.Generate(sourceMeta, group.Descriptors.Where(m => !m.WithOutTargetInstance)))
            {
                yield return methodBody;
            }
        }
    }

    #endregion Private 方法
}
