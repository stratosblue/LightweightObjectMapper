using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using LightweightObjectMapper.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LightweightObjectMapper;

/// <summary>
/// 类型转换匹配器
/// </summary>
internal class TypeConversionMatcher
{
    #region Private 字段

    private readonly BuildContext _context;

#pragma warning disable RS1024 // 正确比较符号

    /// <summary>
    /// 类型匹配字典
    /// </summary>
    private readonly ConcurrentDictionary<ITypeSymbol, ConcurrentDictionary<ITypeSymbol, TypeConversion>> _typeMatchMap = new(SymbolEqualityComparer.Default);

    #endregion Private 字段

#pragma warning restore RS1024 // 正确比较符号

    #region Public 构造函数

    public TypeConversionMatcher(BuildContext buildContext)
    {
        _context = buildContext ?? throw new ArgumentNullException(nameof(buildContext));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 获取映射类型准备
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public TypeConversion GetMappingPrepareTypeConversion(ITypeSymbol fromType, ITypeSymbol toType)
    {
        if (_context.MappingProfiles.TryGetValue(new(fromType, toType), out var profileItem)
            && profileItem.HasProfile
            && profileItem.HasMappingPrepare)
        {
            return new(profileItem.MappingPrepareInvokeDescriptor!);
        }
        return default;
    }

    /// <summary>
    /// 获取类型转换信息
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="enableImplicit"></param>
    /// <returns></returns>
    public TypeConversion GetTypeConversion(ITypeSymbol fromType, ITypeSymbol toType, bool enableImplicit = true)
    {
        if (_typeMatchMap.TryGetValue(fromType, out var suitableMap)
            && suitableMap.TryGetValue(toType, out var typeConversion))
        {
            return typeConversion;
        }

#pragma warning disable RS1024 // 正确比较符号

        suitableMap ??= _typeMatchMap.GetOrAdd(fromType, _ => new(SymbolEqualityComparer.Default));

#pragma warning restore RS1024 // 正确比较符号

        if (_context.MappingProfiles.TryGetValue(new(fromType, toType), out var profileItem)
            && profileItem.HasProfile
            && profileItem.HasTypeMapping)  //对应类型有映射配置
        {
            typeConversion = new(profileItem.TypeMappingInvokeDescriptor!);
        }
        else if (toType.EqualsDefault(_context.WellknownTypes.Object)) //转换到object
        {
            if (enableImplicit)
            {
                typeConversion = new(true);
            }
            else
            {
                typeConversion = new(new MethodInvokeDescriptor("(object)", "((object){0})"));
            }
        }
        else if (toType.EqualsDefault(_context.WellknownTypes.String)
                 && !fromType.EqualsDefault(_context.WellknownTypes.String))  //由非string对象转换到string
        {
            typeConversion = new(new MethodInvokeDescriptor("ToString", "{0}.ToString()"));
        }
        else if (TryGetIEnumerableMatchable(fromType, toType, out var enumerableTypeConversion))  //是否可以按集合转换
        {
            typeConversion = enumerableTypeConversion;
        }
        else if (TryGetTypeMapDescriptor(out var typeMapDescriptor)
                 && typeMapDescriptor is not null)  //有MapTo方法
        {
            var conversionInvokeFormat = $"{{0}}.{Constants.MapMethodName}<{typeMapDescriptor.TargetType.ToFullCodeString()}>()";
            typeConversion = new(new MethodInvokeDescriptor(Constants.MapMethodName, conversionInvokeFormat));
        }
        else if (fromType.EqualsDefault(toType))    //同类型，最后比较，以处理集合类型等
        {
            typeConversion = new(true);
        }
        else    //其它类型
        {
            var conversion = _context.Compilation.ClassifyConversion(fromType, toType);

            if (!conversion.Exists) //不能转换
            {
                typeConversion = new(false);
            }
            else if (conversion.IsImplicit) //隐式转换
            {
                if (enableImplicit)
                {
                    typeConversion = new(true);
                }
                else
                {
                    typeConversion = new(new MethodInvokeDescriptor($"({toType.ToFullCodeString()})", $"(({toType.ToFullCodeString()}){{0}})"));
                }
            }
            else if (conversion.IsExplicit && !conversion.IsUnboxing)   //显式转换且不是拆箱
            {
                if (_context.WellknownTypes.TryGetIEnumerableItemGenericType(fromType, out var fromTypeIEnumerableInterfaceTypeSymbol)
                    && _context.WellknownTypes.TryGetIEnumerableItemGenericType(toType, out var toTypeIEnumerableInterfaceTypeSymbol))
                {
                    //ClassifyConversion 对集合的转换判定好像有误
                    typeConversion = new(false);
                }
                else
                {
                    typeConversion = new(new MethodInvokeDescriptor($"({toType.ToFullCodeString()})", $"({toType.ToFullCodeString()}){{0}}"));
                }
            }
            else
            {
                typeConversion = new(false);
            }
        }

        suitableMap[toType] = typeConversion;

        return typeConversion;

        bool TryGetTypeMapDescriptor(out TypeMapDescriptor? typeMapDescriptor)
        {
            //只查找无实例映射
            if (_context.TypeMapDescriptors.TryGetValue(new TypeMapDescriptor(fromType, toType, true), out typeMapDescriptor))
            {
                return true;
            }

            return false;
        }
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 是否可以按 <see cref="IEnumerable{T}"/> 进行映射
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    protected bool TryGetIEnumerableMatchable(ITypeSymbol fromType, ITypeSymbol toType, out TypeConversion typeConversion)
    {
        var fromInterfaces = InsertHead(fromType.AllInterfaces, fromType);
        fromInterfaces = fromInterfaces.Where(_context.WellknownTypes.IsDeriveFromIEnumerableTDirectly)
                                       .ToImmutableArray();

        typeConversion = default;

        if (fromInterfaces.Length > 0)
        {
            if (toType.Kind == SymbolKind.ArrayType
                && toType is IArrayTypeSymbol arrayTypeSymbol
                && arrayTypeSymbol.ElementType is INamedTypeSymbol toArrayItemTypeSymbol)    //转换到数组
            {
                foreach (var fromInterface in fromInterfaces)
                {
                    if (TryAnalysisAsGenericCollection(fromInterface, out var fromCollectionTypeSymbol, out var fromItemTypeSymbol)
                        && fromItemTypeSymbol is not null)
                    {
                        var itemTypeConversion = GetTypeConversion(fromItemTypeSymbol, toArrayItemTypeSymbol, false);
                        if (itemTypeConversion.CanConversion)   //元素可映射
                        {
                            //TODO 取消魔法值？
                            var methodInvokeDescriptor = new MethodInvokeDescriptor("ToArray", "{0}?.ToArray()");
                            typeConversion = new TypeConversion(FormatCollectionInvokeDescriptorWithItem(new(methodInvokeDescriptor), itemTypeConversion));
                            return true;
                        }
                    }
                }
            }
            else if (TryAnalysisAsGenericCollection(toType, out var toCollectionTypeSymbol, out var toItemTypeSymbol)
                     && toCollectionTypeSymbol is not null
                     && toItemTypeSymbol is not null)   //转换到其它集合
            {
                foreach (var fromInterface in fromInterfaces)
                {
                    if (TryAnalysisAsGenericCollection(fromInterface, out var fromCollectionTypeSymbol, out var fromItemTypeSymbol)
                        && fromItemTypeSymbol is not null)
                    {
                        var itemTypeConversion = GetTypeConversion(fromItemTypeSymbol, toItemTypeSymbol, false);
                        if (itemTypeConversion.CanConversion
                            && _context.CollectionMappingProfiles.TryGetValue(toCollectionTypeSymbol, out var collectionMappingProfileItem))   //元素可映射
                        {
                            typeConversion = new TypeConversion(FormatCollectionInvokeDescriptorWithItem(collectionMappingProfileItem, itemTypeConversion));
                            return true;
                        }
                    }
                }
            }
        }
        return false;

        static ImmutableArray<INamedTypeSymbol> InsertHead(ImmutableArray<INamedTypeSymbol> source, ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return source.Insert(0, namedTypeSymbol);
            }
            return source;
        }

        static MethodInvokeDescriptor FormatCollectionInvokeDescriptorWithItem(CollectionMappingProfileItem collectionMappingProfileItem, TypeConversion itemTypeConversion)
        {
            if (itemTypeConversion.ConversionInvokeDescriptor is not null)
            {
                var conversionInvokeFormat = $"{{0}}.Select(static m => {itemTypeConversion.ConversionInvokeDescriptor.Value.FormatFullInvoke("m")})";
                var invokeFormat = collectionMappingProfileItem.InvokeDescriptor!.Value.FormatFullInvoke(conversionInvokeFormat);
                return new MethodInvokeDescriptor("Select", invokeFormat);
            }

            return collectionMappingProfileItem.InvokeDescriptor!.Value;
        }
    }

    private static bool TryAnalysisAsGenericCollection(ITypeSymbol sourceTypeSymbol, out INamedTypeSymbol? collectionTypeSymbol, out INamedTypeSymbol? itemTypeSymbol)
    {
        if (sourceTypeSymbol is INamedTypeSymbol namedSourceTypeSymbol
            && namedSourceTypeSymbol.IsGenericType
            && namedSourceTypeSymbol.TypeArguments.Length == 1)
        {
            collectionTypeSymbol = namedSourceTypeSymbol.OriginalDefinition;
            itemTypeSymbol = namedSourceTypeSymbol.TypeArguments.First() as INamedTypeSymbol;
            return collectionTypeSymbol is not null && itemTypeSymbol is not null;
        }
        collectionTypeSymbol = null;
        itemTypeSymbol = null;
        return false;
    }

    #endregion Protected 方法
}
