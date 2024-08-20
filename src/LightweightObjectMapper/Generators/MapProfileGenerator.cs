using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using LightweightObjectMapper.Models;
using LightweightObjectMapper.Util;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightweightObjectMapper.Generators;

internal class MapProfileGenerator
{
    #region Private 委托

    private delegate void MappingMethodGenerateCallback(MappingPair mappingPair, MethodInvokeDescriptor invokeDescriptor);

    #endregion Private 委托

    #region Public 属性

    public BuildContext Context { get; }

    public ImmutableArray<ClassDeclarationSyntaxWithSymbol> MappingProfiles { get; }

    #endregion Public 属性

    #region Public 构造函数

    public MapProfileGenerator(BuildContext buildContext, IEnumerable<ClassDeclarationSyntaxWithSymbol> mappingProfiles)
    {
        if (mappingProfiles is null)
        {
            throw new ArgumentNullException(nameof(mappingProfiles));
        }

        Context = buildContext ?? throw new ArgumentNullException(nameof(buildContext));
        MappingProfiles = mappingProfiles.ToImmutableArray();
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Generate()
    {
        var cancellationToken = Context.CancellationToken;

        //为非预定义配置排序
        var allMappingProfiles = MappingProfiles.Where(m => !m.Symbol.EqualsDefault(Context.SelfTypes.PredefinedSpecialTypeMapping))
                                                .SelectMany(m => MappingProfileIncludeUtil.FindAllIncludeMappingProfiles(m.Symbol, Context))
                                                .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var orderedNotPreMappingProfiles = allMappingProfiles.SortByDependencies(m => MappingProfileIncludeUtil.GetIncludeProfiles(m, Context))
                                                             .Where(m => !m.EqualsDefault(Context.SelfTypes.PredefinedSpecialTypeMapping));

        //将预定义配置放到处理列表的最末尾
        var orderedAllMappingProfiles = MappingProfiles.Where(m => m.Symbol.EqualsDefault(Context.SelfTypes.PredefinedSpecialTypeMapping))
                                                       .Select(m => m.Symbol)
                                                       .Concat(orderedNotPreMappingProfiles)
                                                       .Reverse()
                                                       .ToArray();

        foreach (var profileTypeSymbol in orderedAllMappingProfiles)
        {
            var classDeclaration = MappingProfiles.FirstOrDefault(m => m.Symbol.EqualsDefault(profileTypeSymbol)).Syntax;

            if (classDeclaration is null)   //从所有 MappingProfiles 中找不到类型语法，则其为已编译的 MappingProfile
            {
                LoadProfileFromCompiledProfileClass(profileTypeSymbol);
                continue;
            }

            var methodDeclarations = classDeclaration.Members.OfType<MethodDeclarationSyntax>();

            var semanticModel = Context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            var methodSymbolDeclarationMap = methodDeclarations.ToDictionary(m => semanticModel.GetDeclaredSymbol(m, cancellationToken)!, m => m!)!;

            if (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                Context.TryReportTypeDiagnostic(classDeclaration, DiagnosticDescriptors.Error.NeedPartialKeyword, new object[] { profileTypeSymbol.ToFullCodeString() });
                continue;
            }

            var selfTypes = Context.SelfTypes;

            var typeMappings = profileTypeSymbol.AllInterfaces.Where(m => selfTypes.ITypeMapping.EqualsDefault(m.OriginalDefinition)).ToImmutableArray();
            var mappingPrepares = profileTypeSymbol.AllInterfaces.Where(m => selfTypes.IMappingPrepare.EqualsDefault(m.OriginalDefinition)).ToImmutableArray();
            var postMappings = profileTypeSymbol.AllInterfaces.Where(m => selfTypes.IPostMapping.EqualsDefault(m.OriginalDefinition)).ToImmutableArray();
            var typeMemberIgnoreMappings = profileTypeSymbol.AllInterfaces.Where(m => selfTypes.ITypeMemberIgnoreMapping.EqualsDefault(m.OriginalDefinition)).ToImmutableArray();

            var collectionMappingMethods = profileTypeSymbol.GetAllMembers()
                                                            .OfType<IMethodSymbol>()
                                                            .Where(m => m.GetAttributes().Any(m => Context.SelfTypes.CollectionMappingAttribute.EqualsDefault(m.AttributeClass)));

            var validCollectionMappingMethods = new Dictionary<INamedTypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);

            foreach (var item in collectionMappingMethods)
            {
                if (!item.IsGenericMethod
                    || item.TypeArguments.Length != 1)
                {
                    methodSymbolDeclarationMap.TryGetValue(item, out var value);
                    Context.TryReportTypeDiagnostic(value, DiagnosticDescriptors.Error.CollectionMappingMethodDefineError, new object[] { item.ToDisplayString() });
                    continue;
                }
                if (item.Parameters.Length != 1
                    || item.Parameters.First().Type is not INamedTypeSymbol parameterType
                    || !parameterType.IsGenericType
                    || parameterType.TypeArguments.Length != 1
                    || parameterType.TypeArguments.First().Kind != SymbolKind.TypeParameter)
                {
                    methodSymbolDeclarationMap.TryGetValue(item, out var value);
                    Context.TryReportTypeDiagnostic(value, DiagnosticDescriptors.Error.CollectionMappingMethodDefineError, new object[] { item.ToDisplayString() });
                    continue;
                }

                if (item.ReturnType is not INamedTypeSymbol returnType
                    || !returnType.IsGenericType
                    || returnType.TypeArguments.Length != 1
                    || returnType.TypeArguments.First().Kind != SymbolKind.TypeParameter
                    || !TryGetTargetIEnumerableType(returnType, out var targetType)
                    || targetType is null)
                {
                    methodSymbolDeclarationMap.TryGetValue(item, out var value);
                    Context.TryReportTypeDiagnostic(value, DiagnosticDescriptors.Error.CollectionMappingMethodDefineError, new object[] { item.ToDisplayString() });
                    continue;
                }
                if (validCollectionMappingMethods.ContainsKey(targetType))
                {
                    methodSymbolDeclarationMap.TryGetValue(item, out var value);
                    Context.TryReportTypeDiagnostic(value, DiagnosticDescriptors.Warning.DuplicateDefinitionCollectionMappingMethod, new object[] { targetType.ToDisplayString() });
                }
                validCollectionMappingMethods[targetType] = item;
            }

            var builder = new StringBuilder(Constants.CodeFileHeader, 4096);

            foreach (var usingItem in classDeclaration.SyntaxTree.GetCompilationUnitRoot().Usings)
            {
                builder.AppendLine(usingItem.ToString());
            }

            var memberIgnoreMappingMetas = GetTypeMemberIgnoreMappingMetas(profileTypeSymbol, typeMemberIgnoreMappings, methodSymbolDeclarationMap);

            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Runtime.CompilerServices;");

            builder.AppendLine($@"namespace {profileTypeSymbol.ContainingNamespace}
{{
    sealed partial class {profileTypeSymbol.Name}
    {{");

            foreach (var item in memberIgnoreMappingMetas.Where(static m => !m.IsEmpty))
            {
                builder.AppendLine($"[MappingMetadata(MappingMetadataType.{MappingMetadataType.TypeIgnoreMembersDeclaration}, typeof({item.TargetType.ToFullCodeString()}), {string.Join(", ", item.MemberNames.Select(m => $"\"{m}\""))})]\r\n");
            }

            builder.AppendLine($@"public static partial class {Constants.GenerateNestedClassName}
        {{");

            GenerateStaticMappingProfileMethod(profileTypeSymbol,
                                               typeMappings,
                                               methodSymbolDeclarationMap,
                                               builder,
                                               metadataType: MappingMetadataType.TypeMappingDeclaration,
                                               (_, invokeDescriptor) => new(typeMappingInvokeDescriptor: invokeDescriptor),
                                               (_, ev, invokeDescriptor) => new(ev, typeMappingInvokeDescriptor: invokeDescriptor));

            GenerateStaticMappingProfileMethod(profileTypeSymbol,
                                               mappingPrepares,
                                               methodSymbolDeclarationMap,
                                               builder,
                                               metadataType: MappingMetadataType.MappingPrepareDeclaration,
                                               (_, invokeDescriptor) => new(mappingPrepareInvokeDescriptor: invokeDescriptor),
                                               (_, ev, invokeDescriptor) => new(ev, mappingPrepareInvokeDescriptor: invokeDescriptor));

            GenerateStaticMappingProfileMethod(profileTypeSymbol,
                                               postMappings,
                                               methodSymbolDeclarationMap,
                                               builder,
                                               metadataType: MappingMetadataType.PostMappingDeclaration,
                                               (_, invokeDescriptor) => new(postMappingInvokeDescriptor: invokeDescriptor),
                                               (_, ev, invokeDescriptor) => new(ev, postMappingInvokeDescriptor: invokeDescriptor));

            GenerateCollectionMappingProfileMethod(validCollectionMappingMethods, methodSymbolDeclarationMap, builder, profileTypeSymbol);

            builder.AppendLine(@$"        }}
    }}
}}");

            Context.Context.AddSource($"{profileTypeSymbol.Name}.{Constants.GenerateNestedClassName}.cs", CodeFormatUtil.Format(builder.ToString(), cancellationToken));
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static MappingPair CreateMappingPair(INamedTypeSymbol typeMapping)
    {
        return new MappingPair((INamedTypeSymbol)typeMapping.TypeArguments[0], (INamedTypeSymbol)typeMapping.TypeArguments[1]);
    }

    private void GenerateCollectionMappingProfileMethod(Dictionary<INamedTypeSymbol, IMethodSymbol> collectionMappingTypeMap,
                                                        Dictionary<IMethodSymbol, MethodDeclarationSyntax> methodSymbolDeclarationMap,
                                                        StringBuilder builder,
                                                        INamedTypeSymbol profileTypeSymbol)
    {
        var iEnumerableTTypeSymbol = Context.WellknownTypes.IEnumerableT;

        foreach (var item in collectionMappingTypeMap)
        {
            var targetCollectionTypeSymbol = item.Key;
            if (!methodSymbolDeclarationMap.TryGetValue(item.Value, out var methodDeclaration))
            {
                throw new InvalidOperationException($"Can not get the code of method \"{item.Value.ToDisplayString()}\".");
            }

            var genMethodName = $"{Constants.CollectionMappingMethodNamePrefix}{targetCollectionTypeSymbol.GetUniqueId()}";

            var typeArgumentName = item.Value.TypeArguments.First().Name;

            builder.AppendLine(@$"
/// <summary>
/// {MappingMetadataType.CollectionMappingDeclaration} for <see cref=""{targetCollectionTypeSymbol.ToRemarkCodeString()}""/>
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
[MappingMetadata(MappingMetadataType.{MappingMetadataType.CollectionMappingDeclaration}, typeof({targetCollectionTypeSymbol.ToGenericNoTypeArgumentFullCodeString()}))]
public static {targetCollectionTypeSymbol.ContainingNamespace.ToFullCodeString()}.{targetCollectionTypeSymbol.Name}<{typeArgumentName}> {genMethodName}<{typeArgumentName}>({iEnumerableTTypeSymbol.ToFullCodeString()} {methodDeclaration.ParameterList.Parameters.First().Identifier.ValueText})
{methodDeclaration.Body!.ToFullString()}");

            var generatedClassName = $"{profileTypeSymbol.Name}.{Constants.GenerateNestedClassName}";
            var generatedClassFullName = $"{profileTypeSymbol.ToFullCodeString()}.{Constants.GenerateNestedClassName}";

            var invokeFormat = $"{genMethodName}({{0}})";
            var invokeDescriptor = new MethodInvokeDescriptor(generatedClassName, generatedClassFullName, genMethodName, invokeFormat);

            Context.CollectionMappingProfiles.AddOrUpdate(targetCollectionTypeSymbol,
                                                          _ => new(invokeDescriptor: invokeDescriptor),
                                                          (_, ev) => new(ev, invokeDescriptor: invokeDescriptor));
        }
    }

    private void GenerateStaticMappingProfileMethod(INamedTypeSymbol profileTypeSymbol,
                                                    ImmutableArray<INamedTypeSymbol> interfaces,
                                                    Dictionary<IMethodSymbol, MethodDeclarationSyntax> methodSymbolDeclarationMap,
                                                    StringBuilder builder,
                                                    MappingMetadataType metadataType,
                                                    Func<MappingPair, MethodInvokeDescriptor, MappingProfileItem> addValueFactory,
                                                    Func<MappingPair, MappingProfileItem, MethodInvokeDescriptor, MappingProfileItem> updateValueFactory)
    {
        var methodPrefix = metadataType switch
        {
            MappingMetadataType.TypeMappingDeclaration => Constants.TypeMappingMethodNamePrefix,
            MappingMetadataType.MappingPrepareDeclaration => Constants.MappingPrepareMethodNamePrefix,
            MappingMetadataType.PostMappingDeclaration => Constants.PostMappingMethodNamePrefix,
            _ => throw new ArgumentException(nameof(metadataType))
        };

        foreach (var mappingInterface in interfaces)
        {
            var mappingMethod = mappingInterface.GetMembers().First();

            var mappingPair = CreateMappingPair(mappingInterface);

            if (profileTypeSymbol.FindImplementationForInterfaceMember(mappingMethod) is not IMethodSymbol mappingMethodImplSymbol)
            {
                throw new InvalidOperationException($"Can not found implement of interface \"{mappingMethod.ToDisplayString()}\" in \"{profileTypeSymbol.ToDisplayString()}\".");
            }
            if (!methodSymbolDeclarationMap.TryGetValue(mappingMethodImplSymbol, out var methodDeclaration))
            {
                throw new InvalidOperationException($"Can not found implement code of interface \"{mappingMethod.ToDisplayString()}\" in \"{profileTypeSymbol.ToDisplayString()}\".");
            }

            var fromType = mappingInterface.TypeArguments[0];
            var toType = mappingInterface.TypeArguments[1];

            var genMethodName = $"{methodPrefix}{CrcUtil.Hash(fromType, toType)}";

            var fields = Array.Empty<object>();

            if (metadataType == MappingMetadataType.MappingPrepareDeclaration)
            {
                fields = methodDeclaration.Body is not null
                         ? CollectAssignmentFieldNames(methodDeclaration.Body)
                         : CollectArrayCreationAssignmentFieldNames(methodDeclaration.ExpressionBody!);
            }

            builder.AppendLine(@$"
/// <summary>
/// {metadataType} for <see cref=""{fromType.ToRemarkCodeString()}""/> to <see cref=""{toType.ToRemarkCodeString()}""/>
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
[MappingMetadata(MappingMetadataType.{metadataType}, typeof({fromType.ToFullCodeString()}), typeof({toType.ToFullCodeString()}))]{(fields.Length > 0 ? $"\n[MappingMetadata(MappingMetadataType.{MappingMetadataType.IgnoreMembersDeclaration}, {string.Join(", ", fields.Select(m => $"\"{m}\""))})]" : string.Empty)}
public static {mappingMethodImplSymbol.ReturnType.ToFullCodeString()} {genMethodName}({GenerateMethodParameterString(mappingMethodImplSymbol.Parameters)})");

            builder.AppendLine(methodDeclaration.Body?.ToFullString() ?? $"{methodDeclaration.ExpressionBody!.ToFullString()};");

            var generatedClassName = $"{profileTypeSymbol.Name}.{Constants.GenerateNestedClassName}";
            var generatedClassFullName = $"{profileTypeSymbol.ToFullCodeString()}.{Constants.GenerateNestedClassName}";

            var invokeFormat = $"{genMethodName}({GenerateInvokeFormatParameterString(mappingMethodImplSymbol.Parameters)})";
            var invokeDescriptor = new MethodInvokeDescriptor(generatedClassName, generatedClassFullName, genMethodName, invokeFormat, fields);

            Context.MappingProfiles.AddOrUpdate(mappingPair,
                                                mappingPair => addValueFactory(mappingPair, invokeDescriptor),
                                                (mappingPair, ev) => updateValueFactory(mappingPair, ev, invokeDescriptor));
        }

        object[] CollectAssignmentFieldNames(BlockSyntax blockSyntax)
        {
            //只检查最后一个返回表达式为 new 的初始化属性，其余逻辑太复杂，不检查
            if (blockSyntax.Statements.Reverse().OfType<ReturnStatementSyntax>().FirstOrDefault() is ReturnStatementSyntax returnStatementSyntax
                && returnStatementSyntax.Expression is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                return CollectObjectCreationAssignmentFieldNames(objectCreationExpressionSyntax);
            }
            return Array.Empty<object>();
        }

        object[] CollectArrayCreationAssignmentFieldNames(ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
        {
            //只检查最后一个返回表达式为 new 的初始化属性，其余逻辑太复杂，不检查
            if (arrowExpressionClauseSyntax.Expression is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                return CollectObjectCreationAssignmentFieldNames(objectCreationExpressionSyntax);
            }
            return Array.Empty<object>();
        }

        object[] CollectObjectCreationAssignmentFieldNames(ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
        {
            object[] result = [];
            //检查构造函数
            if (objectCreationExpressionSyntax.ArgumentList?.Arguments.Count > 0)
            {
                var semanticModel = Context.Compilation.GetSemanticModel(objectCreationExpressionSyntax.SyntaxTree);

                var symbolInfo = semanticModel.GetSymbolInfo(objectCreationExpressionSyntax, default);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.Parameters.Length > 0)
                {
                    result = methodSymbol.Parameters.Select(m => m.Name)
                                                    .ToArray();
                }
            }

            //只检查最后一个返回表达式为 new 的初始化属性，其余逻辑太复杂，不检查
            if (objectCreationExpressionSyntax.Initializer is InitializerExpressionSyntax initializerExpressionSyntax
                && initializerExpressionSyntax.Expressions.Count > 0)
            {
                //仅检查赋值语句
                result = initializerExpressionSyntax.Expressions.OfType<AssignmentExpressionSyntax>()
                                                                .Where(m => m.Left is IdentifierNameSyntax)
                                                                .Select(m => ((IdentifierNameSyntax)m.Left).Identifier.ValueText as object)
                                                                .Concat(result)
                                                                .Distinct()
                                                                .ToArray();
            }
            return result;
        }
    }

    private List<TypeMemberIgnoreMappingMeta> GetTypeMemberIgnoreMappingMetas(INamedTypeSymbol profileTypeSymbol,
                                                                              ImmutableArray<INamedTypeSymbol> typeMemberIgnoreMappingInterfaces,
                                                                              Dictionary<IMethodSymbol, MethodDeclarationSyntax> methodSymbolDeclarationMap)
    {
        var dictionary = new Dictionary<ITypeSymbol, TypeMemberIgnoreMappingMeta>(SymbolEqualityComparer.Default);
        foreach (var mappingInterface in typeMemberIgnoreMappingInterfaces)
        {
            var mappingMethod = mappingInterface.GetMembers().First();

            if (profileTypeSymbol.FindImplementationForInterfaceMember(mappingMethod) is not IMethodSymbol mappingMethodImplSymbol)
            {
                throw new InvalidOperationException($"Can not found implement of interface \"{mappingMethod.ToDisplayString()}\" in \"{profileTypeSymbol.ToDisplayString()}\".");
            }
            if (!methodSymbolDeclarationMap.TryGetValue(mappingMethodImplSymbol, out var methodDeclaration))
            {
                throw new InvalidOperationException($"Can not found implement code of interface \"{mappingMethod.ToDisplayString()}\" in \"{profileTypeSymbol.ToDisplayString()}\".");
            }

            var targetTypeSymbol = mappingMethodImplSymbol.Parameters.First().Type;

            var accessedMembersCollector = new AccessedMembersCollector(methodDeclaration.ParameterList.Parameters.First().Identifier);
            methodDeclaration.Body?.Accept(accessedMembersCollector);
            if (accessedMembersCollector.AccessedMemberNames.Count == 0)
            {
                continue;
            }

            if (!dictionary.TryGetValue(targetTypeSymbol, out var memberIgnoreMappingMeta))
            {
                memberIgnoreMappingMeta = new TypeMemberIgnoreMappingMeta(targetTypeSymbol);
                dictionary.Add(targetTypeSymbol, memberIgnoreMappingMeta);
            }

            memberIgnoreMappingMeta.MemberNames.AddRange(accessedMembersCollector.AccessedMemberNames);
        }

        foreach (var item in dictionary)
        {
            var ignoreMappingMeta = Context.TypeIgnoreMemberMetas.GetOrAdd(item.Key, type => new(type));
            ignoreMappingMeta.MemberNames.AddRange(item.Value.MemberNames);
        }

        return dictionary.Values.ToList();
    }

    /// <summary>
    /// 从已编译的类中加载配置
    /// </summary>
    /// <param name="profileTypeSymbol"></param>
    private void LoadProfileFromCompiledProfileClass(INamedTypeSymbol profileTypeSymbol)
    {
        var generateNestedType = profileTypeSymbol.GetTypeMembers().FirstOrDefault(m => m.Name.Equals(Constants.GenerateNestedClassName, StringComparison.Ordinal));

        if (generateNestedType is null)
        {
            Context.TryReportTypeDiagnostic(DiagnosticDescriptors.Error.ErrorMappingProfileInclude, new object[] { profileTypeSymbol.ToFullCodeString() });
            return;
        }

        var mappingMetadataAttributeFullName = Context.SelfTypes.MappingMetadataAttribute.ToFullCodeString();

        var methodSymbols = generateNestedType.GetMembers()
                                              .OfType<IMethodSymbol>()
                                              .Select(methodSymbol =>
                                              {
                                                  var mappingMetadatas = methodSymbol.GetAttributes()
                                                                                     //.Where(m => Context.SelfTypes.MappingMetadataAttribute.EqualsDefault(m.AttributeClass)) //不同导入位置时，不相等，使用名称进行对比
                                                                                     .Where(m => string.Equals(mappingMetadataAttributeFullName, m.AttributeClass?.ToFullCodeString(), StringComparison.Ordinal))
                                                                                     .ToArray();

                                                  var typeAttributeData = mappingMetadatas.FirstOrDefault(m => ((MappingMetadataType)m.ConstructorArguments.First().Value!) != MappingMetadataType.IgnoreMembersDeclaration);

                                                  if (typeAttributeData is null)
                                                  {
                                                      return null;
                                                  }

                                                  var fields = mappingMetadatas.Where(m => ((MappingMetadataType)m.ConstructorArguments.First().Value!) == MappingMetadataType.IgnoreMembersDeclaration)
                                                                               .SelectMany(m => m.ConstructorArguments[1].Values)
                                                                               .Select(m => (string)m.Value!)
                                                                               .ToArray();

                                                  var typeSymbols = typeAttributeData.ConstructorArguments.Skip(1)
                                                                                                          .First().Values
                                                                                                          .OfType<TypedConstant>()
                                                                                                          .SelectMany(m => m.Kind == TypedConstantKind.Array ? m.Values.Select(m => m.Value) : new[] { m.Value })
                                                                                                          .OfType<ITypeSymbol>()
                                                                                                          .ToArray();

                                                  return new ProfileLoadMethodMeta((MappingMetadataType)typeAttributeData.ConstructorArguments.First().Value!,
                                                                                   methodSymbol,
                                                                                   mappingMetadatas,
                                                                                   fields,
                                                                                   typeSymbols);
                                              })
                                              .OfType<ProfileLoadMethodMeta>()
                                              .Where(m => m.MappingMetadatas.Length > 0)
                                              .ToArray()!;

        LoadProfileMethods(methodMetas: methodSymbols.Where(m => m.MetadataType == MappingMetadataType.TypeMappingDeclaration),
                           profileTypeSymbol: generateNestedType,
                           addValueFactory: (_, invokeDescriptor) => new(typeMappingInvokeDescriptor: invokeDescriptor),
                           updateValueFactory: (_, invokeDescriptor, ev) => ev.HasTypeMapping ? ev : new MappingProfileItem(ev, typeMappingInvokeDescriptor: invokeDescriptor));

        LoadProfileMethods(methodMetas: methodSymbols.Where(m => m.MetadataType == MappingMetadataType.MappingPrepareDeclaration),
                           profileTypeSymbol: generateNestedType,
                           addValueFactory: (_, invokeDescriptor) => new(mappingPrepareInvokeDescriptor: invokeDescriptor),
                           updateValueFactory: (_, invokeDescriptor, ev) => ev.HasMappingPrepare ? ev : new MappingProfileItem(ev, mappingPrepareInvokeDescriptor: invokeDescriptor));

        LoadProfileMethods(methodMetas: methodSymbols.Where(m => m.MetadataType == MappingMetadataType.PostMappingDeclaration),
                           profileTypeSymbol: generateNestedType,
                           addValueFactory: (_, invokeDescriptor) => new(postMappingInvokeDescriptor: invokeDescriptor),
                           updateValueFactory: (_, invokeDescriptor, ev) => ev.HasPostMapping ? ev : new MappingProfileItem(ev, postMappingInvokeDescriptor: invokeDescriptor));

        foreach (var methodMeta in methodSymbols.Where(m => m.MetadataType == MappingMetadataType.CollectionMappingDeclaration))
        {
            var collectionType = methodMeta.TypeSymbols.First().OriginalDefinition;

            var generatedClassName = $"{profileTypeSymbol.Name}.{Constants.GenerateNestedClassName}";
            var generatedClassFullName = $"{profileTypeSymbol.ToFullCodeString()}.{Constants.GenerateNestedClassName}";

            var methodName = methodMeta.MethodSymbol.Name;

            var invokeFormat = $"{methodName}({{0}})";
            var invokeDescriptor = new MethodInvokeDescriptor(generatedClassName, generatedClassFullName, methodName, invokeFormat);

            Context.CollectionMappingProfiles.AddOrUpdate(collectionType,
                                                          _ => new(invokeDescriptor: invokeDescriptor),
                                                          (_, ev) => new(ev, invokeDescriptor: invokeDescriptor));
        }

        var typeIgnoreMembersDeclarationsAttributes = generateNestedType.GetAttributes()
                                                                        .Where(m => ((MappingMetadataType)m.ConstructorArguments.First().Value!) == MappingMetadataType.TypeIgnoreMembersDeclaration);

        foreach (var item in typeIgnoreMembersDeclarationsAttributes)
        {
            if (item.ConstructorArguments.Length < 2)   //无效
            {
                continue;
            }

            ImmutableArray<TypedConstant> attributeValues = item.ConstructorArguments[1].Values;
            if (attributeValues.Length < 2
                || attributeValues[0].Value is not ITypeSymbol targetTypeSymbol
                || targetTypeSymbol is null) //无效
            {
                continue;
            }

            var memberNames = attributeValues.Skip(1)
                                             .Select(m => m.Value)
                                             .OfType<string>()
                                             .ToArray();

            var ignoreMappingMeta = Context.TypeIgnoreMemberMetas.GetOrAdd(targetTypeSymbol, type => new(type));
            ignoreMappingMeta.MemberNames.AddRange(memberNames);
        }
    }

    private void LoadProfileMethods(IEnumerable<ProfileLoadMethodMeta> methodMetas,
                                    INamedTypeSymbol profileTypeSymbol,
                                    Func<MappingPair, MethodInvokeDescriptor, MappingProfileItem> addValueFactory,
                                    Func<MappingPair, MethodInvokeDescriptor, MappingProfileItem, MappingProfileItem> updateValueFactory)
    {
        foreach (var methodMeta in methodMetas)
        {
            ITypeSymbol sourceType;
            ITypeSymbol targetType;
            switch (methodMeta.MetadataType)
            {
                case MappingMetadataType.MappingPrepareDeclaration:
                case MappingMetadataType.PostMappingDeclaration:
                case MappingMetadataType.TypeMappingDeclaration:
                    sourceType = methodMeta.TypeSymbols[0];
                    targetType = methodMeta.TypeSymbols[1];
                    break;

                case MappingMetadataType.CollectionMappingDeclaration:
                case MappingMetadataType.IgnoreMembersDeclaration:
                default:
                    throw new ArgumentException($"Error type: {methodMeta.MetadataType}");
            }

            var generatedClassName = $"{profileTypeSymbol.Name}";
            var generatedClassFullName = $"{profileTypeSymbol.ToFullCodeString()}";
            var methodName = methodMeta.MethodSymbol.Name;

            var invokeFormat = $"{methodName}({GenerateInvokeFormatParameterString(methodMeta.MethodSymbol.Parameters)})";
            var invokeDescriptor = new MethodInvokeDescriptor(generatedClassName, generatedClassFullName, methodName, invokeFormat);

            Context.MappingProfiles.AddOrUpdate(new(sourceType, targetType),
                                    pair => addValueFactory(pair, invokeDescriptor),
                                    (pair, ev) => updateValueFactory(pair, invokeDescriptor, ev));
        }
    }

    private bool TryGetTargetIEnumerableType(INamedTypeSymbol namedTypeSymbol, out INamedTypeSymbol? targetTypeSymbol)
    {
        if (Context.WellknownTypes.IsDeriveFromIEnumerableTDirectly(namedTypeSymbol))
        {
            targetTypeSymbol = namedTypeSymbol.OriginalDefinition;
            return true;
        }
        if (Context.WellknownTypes.IsDeriveFromIEnumerableT(namedTypeSymbol))
        {
            targetTypeSymbol = namedTypeSymbol.OriginalDefinition;
            return true;
        }
        targetTypeSymbol = null;
        return false;
    }

    #region Util

    private static string GenerateInvokeFormatParameterString(ImmutableArray<IParameterSymbol> parameters)
    {
        return parameters.Length switch
        {
            0 => string.Empty,
            1 => "{0}",
            2 => "{0}, {1}",
            3 => "{0}, {1}, {2}",
            _ => string.Join(", ", Enumerable.Range(0, parameters.Length).Select(i => $"{{{i}}}")),
        };
    }

    private static string GenerateMethodParameterString(ImmutableArray<IParameterSymbol> parameters)
    {
        return string.Join(", ", parameters.Select(parameter => $"{parameter.ToFullCodeString()} {parameter.Name}"));
    }

    #endregion Util

    #endregion Private 方法
}
