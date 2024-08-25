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
        TypeConversionMatcher = buildContext.TypeConversionMatcher;
        IdentifierMatcher = buildContext.IdentifierMatcher;
        MapDescriptorGroups = buildContext.TypeMapDescriptors.Values.GroupBy(NormalizationSourceType)
                                                                    .Select(m => new TypeMapDescriptorGroup(m.Key, m))
                                                                    .ToImmutableArray();
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
        var accessibility = Context.CompilationProperties.MappingMethodAccessibility == Accessibility.Public
                            ? "public"
                            : "internal";
        var codeBuilder = new StringBuilder();

        var className = $"LOMMapExtensions_{Context.Compilation.Assembly.Name.Replace(".", "_")}";

        foreach (var typeMapDescriptorGroup in MapDescriptorGroups)
        {
            codeBuilder.AppendLine(@$"{Constants.CodeFileHeader}

using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace {Constants.RootNamespace}
{{
    {accessibility} static partial class {className}
    {{
");

            foreach (var methodCode in GenerateMethods(typeMapDescriptorGroup).Where(m => !string.IsNullOrWhiteSpace(m)))
            {
                codeBuilder.AppendLine($"{methodCode}{Environment.NewLine}");
            }
            codeBuilder.AppendLine($"}}{Environment.NewLine}}}");

            Context.Context.AddSource(hintName: $"{className}_{GetCodeHintName(typeMapDescriptorGroup.SourceType)}.g.cs",
                                      sourceText: CodeFormatUtil.Format(codeBuilder.ToString(), Context.CancellationToken));

            codeBuilder.Clear();
        }

        string GetCodeHintName(ITypeSymbol sourceType)
        {
            if (!SymbolEqualityComparer.Default.Equals(sourceType, Context.WellknownTypes.IEnumerableT)
                && Context.WellknownTypes.TryGetIEnumerableItemGenericType(sourceType, out var iEnumerableInterfaceTypeSymbol))
            {
                return $"{GetCodeHintName(sourceType.OriginalDefinition)}_{GetCodeHintName(iEnumerableInterfaceTypeSymbol!.TypeArguments[0])}";
            }
            return sourceType.Name.Replace(".", "_");
        }
    }

    #endregion Public 方法

    #region Private 方法

    private IEnumerable<string?> GenerateMethods(TypeMapDescriptorGroup typeMapDescriptorGroup)
    {
        var sourceMeta = Context.GetTypeMapMetaData(typeMapDescriptorGroup.SourceType);

        if (Context.TryReportTypeDiagnostic(typeMapDescriptorGroup.Descriptors.SelectMany(m => m.Nodes), sourceMeta.ReportDiagnosticDescriptor, sourceMeta.DiagnosticMessageArgs))
        {
            yield break;
        }

        var genericMapDescriptors = typeMapDescriptorGroup.Descriptors.Where(m => m.WithOutTargetInstance).ToArray();

        yield return _genericMapMethodGenerator.Generate(sourceMeta, genericMapDescriptors);

        foreach (var methodBody in _specialMapMethodGenerator.Generate(sourceMeta, typeMapDescriptorGroup.Descriptors.Where(m => !m.WithOutTargetInstance)))
        {
            yield return methodBody;
        }
    }

    #endregion Private 方法
}
