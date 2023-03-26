using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using LightweightObjectMapper.Generators;
using LightweightObjectMapper.Models;
using LightweightObjectMapper.Properties;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LightweightObjectMapper;

[Generator(LanguageNames.CSharp)]
public class LightweightObjectMapperSourceGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //System.Diagnostics.Debugger.Launch();

        InitializePreCodes(context);

        var typeMapDescriptorProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterMapMethodInvocationSyntaxNode, TransformMapMethodInvocationSyntaxNode)
                                                              .SelectMany((items, _) => items);

        var mappingProfileProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterMappingProfileSyntaxNode, TransformMappingProfileSyntaxNode)
                                                           .Where(m => m is not null)
                                                           .Select((m, _) => m!.Value);

        var compilationPropertiesProvider = context.AnalyzerConfigOptionsProvider.Select((configOptions, token) =>
        {
            configOptions.GlobalOptions.TryGetValue("build_property.LOMappingMethodAccessibility", out var mappingMethodAccessibilityValue);
            mappingMethodAccessibilityValue = mappingMethodAccessibilityValue?.ToLowerInvariant();
            var mappingMethodAccessibility = mappingMethodAccessibilityValue switch
            {
                "public" => Accessibility.Public,
                _ => Accessibility.Internal,
            };

            return new CompilationProperties()
            {
                MappingMethodAccessibility = mappingMethodAccessibility,
            };
        });

        context.RegisterSourceOutput(mappingProfileProvider.Collect().Combine(typeMapDescriptorProvider.Collect()).Combine(compilationPropertiesProvider),
                                 (context, input) =>
                                 {
                                     var mappingProfiles = input.Left.Left;
                                     var descriptors = input.Left.Right;
                                     var compilationProperties = input.Right;

                                     var allMapDescriptorMap = new Dictionary<TypeMapDescriptor, TypeMapDescriptor>();

                                     for (int i = 0; i < descriptors.Length; i++)
                                     {
                                         var typeMapDescriptor = descriptors[i]!;

                                         if (allMapDescriptorMap.TryGetValue(typeMapDescriptor, out var existedTypeMapDescriptor))
                                         {
                                             existedTypeMapDescriptor.AddNodes(typeMapDescriptor.Nodes);
                                         }
                                         else
                                         {
                                             allMapDescriptorMap.Add(typeMapDescriptor, typeMapDescriptor);
                                         }
                                     }

                                     var compilation = mappingProfiles.FirstOrDefault(m => m.Compilation is not null).Compilation
                                                       ?? allMapDescriptorMap.FirstOrDefault(m => m.Value.Compilation is not null).Value?.Compilation;

                                     var buildContext = compilation is not null
                                                        ? new BuildContext(context, compilation, compilationProperties)
                                                        : null;

                                     if (mappingProfiles.Length > 0)
                                     {
                                         buildContext = ThrowIfBuildContextIsNull(buildContext);
                                         GenerateMappingProfiles(buildContext, mappingProfiles);
                                     }

                                     if (allMapDescriptorMap.Count > 0)
                                     {
                                         buildContext = ThrowIfBuildContextIsNull(buildContext);
                                         foreach (var item in allMapDescriptorMap)
                                         {
                                             buildContext.TypeMapDescriptors[item.Key] = item.Value;
                                         }

                                         GenerateMapExtensionMethods(buildContext, context.CancellationToken);
                                     }
                                 });

        static BuildContext ThrowIfBuildContextIsNull(BuildContext? buildContext)
        {
            if (buildContext is null)
            {
                throw new InvalidOperationException("Can not get the compilation instance.");
            }
            return buildContext;
        }
    }

    #endregion Public 方法

    #region Private 方法

    #region Filter

    private static bool FilterMapMethodInvocationSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax
            && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
            && string.Compare(Constants.MapMethodName, memberAccessExpressionSyntax.Name.Identifier.Text) == 0)
        {
            return true;
        }
        return false;
    }

    private static bool FilterMappingProfileSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Any(m => m.Attributes.Any(m => IsProbablyMappingProfile((m.Name as IdentifierNameSyntax)?.Identifier.Text))))
        {
            return true;
        }

        return false;

        static bool IsProbablyMappingProfile(string? nameText)
        {
            return !string.IsNullOrEmpty(nameText)
                   && (string.Equals("MappingProfile", nameText, StringComparison.Ordinal)
                       || string.Equals(nameof(MappingProfileAttribute), nameText, StringComparison.Ordinal)
                       || string.Equals($"{Constants.RootNamespace}.MappingProfile", nameText, StringComparison.Ordinal)
                       || string.Equals($"{Constants.RootNamespace}.{nameof(MappingProfileAttribute)}", nameText, StringComparison.Ordinal));
        }
    }

    private static IEnumerable<TypeMapDescriptor> TransformMapMethodInvocationSyntaxNode(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)generatorSyntaxContext.Node;

        if (generatorSyntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax) is SymbolInfo mapMethodSymbolInfo
            && mapMethodSymbolInfo.Symbol is IMethodSymbol mapMethodSymbol
            && mapMethodSymbol.IsExtensionMethod
            && mapMethodSymbol.TypeArguments.Length == 1
            && string.Compare(Constants.RootNamespace, mapMethodSymbol.ContainingNamespace.Name) == 0
            && TryGetMapFromType(invocationExpressionSyntax, out var fromTypeSymbol))
        {
            var compilation = generatorSyntaxContext.SemanticModel.Compilation;
            var withOutTargetInstance = invocationExpressionSyntax.ArgumentList.Arguments.Count == 0;
            var toTypeSymbol = mapMethodSymbol.TypeArguments[0];
            if (CollectionTypeMapDescriptor.TryCreate(compilation, fromTypeSymbol!, toTypeSymbol!, withOutTargetInstance, out var collectionTypeMapDescriptor)
                && collectionTypeMapDescriptor is not null)
            {
                //为集合映射的元素类型添加无实例映射
                var itemTypeMapDescriptor = new TypeMapDescriptor(collectionTypeMapDescriptor.SourceItemType, collectionTypeMapDescriptor.TargetItemType, true);
                itemTypeMapDescriptor.AddNode(invocationExpressionSyntax);
                itemTypeMapDescriptor.Compilation = compilation;
                yield return itemTypeMapDescriptor;

                collectionTypeMapDescriptor.AddNode(invocationExpressionSyntax);
                collectionTypeMapDescriptor.Compilation = compilation;
                yield return collectionTypeMapDescriptor;
                yield break;
            }
            else
            {
                var typeMapDescriptor = new TypeMapDescriptor(fromTypeSymbol!, toTypeSymbol, withOutTargetInstance);
                typeMapDescriptor.AddNode(invocationExpressionSyntax);
                typeMapDescriptor.Compilation = compilation;
                yield return typeMapDescriptor;
                yield break;
            }
        }

        bool TryGetMapFromType(InvocationExpressionSyntax invocationExpressionSyntax, out ITypeSymbol? fromTypeSymbol)
        {
            var expressionSyntax = (invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax)?.Expression;
            if (expressionSyntax == null)
            {
                fromTypeSymbol = null;
                return false;
            }

            if (generatorSyntaxContext.SemanticModel.GetSymbolInfo(expressionSyntax) is SymbolInfo parameterSymbolInfo
                && TryGetParameterSymbolType(parameterSymbolInfo, out fromTypeSymbol))
            {
                return true;
            }
            else if (generatorSyntaxContext.SemanticModel.GetTypeInfo(expressionSyntax) is Microsoft.CodeAnalysis.TypeInfo typeInfo)
            {
                fromTypeSymbol = typeInfo.Type;
                return fromTypeSymbol is not null;
            }
            fromTypeSymbol = null;
            return false;
        }

        static bool TryGetParameterSymbolType(SymbolInfo parameterSymbolInfo, out ITypeSymbol? formTypeSymbol)
        {
            formTypeSymbol = parameterSymbolInfo.Symbol switch
            {
                IParameterSymbol parameterSymbol => parameterSymbol.Type,
                IMethodSymbol methodSymbol => methodSymbol.MethodKind switch
                {
                    MethodKind.Constructor => methodSymbol.ReceiverType,
                    _ => methodSymbol.ReturnType,
                },
                ILocalSymbol localSymbol => localSymbol.Type,
                IFieldSymbol fieldSymbol => fieldSymbol.Type,
                IPropertySymbol propertySymbol => propertySymbol.Type,
                _ => null,
            };
            return formTypeSymbol is not null;
        }
    }

    private static ClassDeclarationSyntaxWithSymbol? TransformMappingProfileSyntaxNode(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)generatorSyntaxContext.Node;

        if (generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol)
        {
            return new(classDeclarationSyntax, namedTypeSymbol)
            {
                Compilation = generatorSyntaxContext.SemanticModel.Compilation,
            };
        }

        return null;
    }

    #endregion Filter

    private static void GenerateMapExtensionMethods(BuildContext context, CancellationToken cancellationToken)
    {
        try
        {
            var mapExtensionMethodGenerator = new MapExtensionMethodGenerator(context);
            mapExtensionMethodGenerator.Generate();
        }
        catch (Exception ex)
        {
            context.ReportUnexpectedExceptionDiagnostic(ex);
        }
    }

    private static void GenerateMappingProfiles(BuildContext context, IEnumerable<ClassDeclarationSyntaxWithSymbol> mappingProfiles)
    {
        try
        {
            var mapProfileGenerator = new MapProfileGenerator(context, mappingProfiles);
            mapProfileGenerator.Generate();
        }
        catch (Exception ex)
        {
            context.ReportUnexpectedExceptionDiagnostic(ex);
        }
    }

    private static void InitializePreCodes(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            var assembly = typeof(LightweightObjectMapperSourceGenerator).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            AddPreCodes(context, assembly, resourceNames.Single(m => m.EndsWith(Constants.PreCodesFileName)));
            AddPreCodes(context, assembly, resourceNames.Single(m => m.EndsWith(Constants.PredefinedSpecialTypeMappingFileName)));
        });

        static void AddPreCodes(IncrementalGeneratorPostInitializationContext context, Assembly assembly, string resourceName)
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(resourceStream);
            var code = reader.ReadToEnd();

            //本地化代码
            var resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            foreach (DictionaryEntry item in resourceSet)
            {
                if (item.Key is string key
                    && item.Value is string value)
                {
                    code = code.Replace($"${{{{{key}}}}}", value);
                }
            }

            context.AddSource(Path.GetFileName(resourceName), SourceText.From(code, Encoding.UTF8));
        }
    }

    #endregion Private 方法
}
