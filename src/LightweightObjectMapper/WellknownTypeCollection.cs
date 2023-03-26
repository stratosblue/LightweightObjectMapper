using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

/// <summary>
/// 已知类型集合
/// </summary>
public class WellknownTypeCollection
{
    #region Public 属性

    /// <summary>
    /// <see cref="System.Array"/>
    /// </summary>
    public ITypeSymbol Array { get; }

    /// <summary>
    /// <see cref="IEnumerable{T}"/>
    /// </summary>
    public ITypeSymbol IEnumerableT { get; }

    /// <summary>
    /// <see cref="System.Nullable{T}"/>
    /// </summary>
    public ITypeSymbol NullableT { get; }

    /// <summary>
    /// <see cref="object"/>
    /// </summary>
    public ITypeSymbol Object { get; }

    /// <summary>
    /// <see cref="string"/>
    /// </summary>
    public ITypeSymbol String { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="WellknownTypeCollection"/>
    public WellknownTypeCollection(Compilation compilation)
    {
        Object = compilation.GetSpecialType(SpecialType.System_Object);
        String = compilation.GetSpecialType(SpecialType.System_String);
        IEnumerableT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
        Array = compilation.GetSpecialType(SpecialType.System_Array);
        NullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 尝试获取类型的 <see cref="IEnumerable{T}"/> 接口，其实现的 <see cref="IEnumerable{T}"/> 泛型参数与类型本身泛型参数相同
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <param name="iEnumerableT"></param>
    /// <param name="iEnumerableInterfaceTypeSymbol"></param>
    /// <returns></returns>
    public static bool TryGetIEnumerableItemGenericType(ITypeSymbol typeSymbol, ITypeSymbol iEnumerableT, out INamedTypeSymbol? iEnumerableInterfaceTypeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol
            || !namedTypeSymbol.IsGenericType
            || namedTypeSymbol.TypeArguments.Length != 1
            || namedTypeSymbol.TypeArguments.First() is not ITypeSymbol genericType)
        {
            if (typeSymbol is not IArrayTypeSymbol arrayTypeSymbol)
            {
                iEnumerableInterfaceTypeSymbol = null;
                return false;
            }
            else
            {
                iEnumerableInterfaceTypeSymbol = arrayTypeSymbol.AllInterfaces.Where(m => iEnumerableT.EqualsDefault(m.OriginalDefinition)).FirstOrDefault(m => m.TypeArguments.First().EqualsDefault(arrayTypeSymbol.ElementType));
                return true;
            }
        }

        iEnumerableInterfaceTypeSymbol = namedTypeSymbol.AllInterfaces.Where(m => iEnumerableT.EqualsDefault(m.OriginalDefinition)).FirstOrDefault(m => m.TypeArguments.First().EqualsDefault(genericType));

        if (iEnumerableInterfaceTypeSymbol is null
            && iEnumerableT.EqualsDefault(namedTypeSymbol.OriginalDefinition))
        {
            iEnumerableInterfaceTypeSymbol = namedTypeSymbol;
        }

        return iEnumerableInterfaceTypeSymbol is not null;
    }

    /// <summary>
    /// 检查类型是否是 <see cref="IEnumerable{T}"/> 的派生类型
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public bool IsDeriveFromIEnumerableT(ITypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(IsDeriveFromIEnumerableTDirectly);
    }

    /// <summary>
    /// 检查类型是否是 <see cref="IEnumerable{T}"/> 的直接派生类型
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public bool IsDeriveFromIEnumerableTDirectly(ITypeSymbol typeSymbol)
    {
        return IEnumerableT.EqualsDefault(typeSymbol.OriginalDefinition);
    }

    /// <summary>
    /// 检查类型是否是 <see cref="IEnumerable{T}"/> 的派生，且泛型参数与实现的 <see cref="IEnumerable{T}"/> 相同的泛型类型
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public bool IsIEnumerableItemGenericType(ITypeSymbol typeSymbol) => TryGetIEnumerableItemGenericType(typeSymbol, IEnumerableT, out _);

    /// <inheritdoc cref="TryGetIEnumerableItemGenericType(ITypeSymbol, ITypeSymbol, out INamedTypeSymbol?)"/>
    public bool TryGetIEnumerableItemGenericType(ITypeSymbol typeSymbol, out INamedTypeSymbol? iEnumerableInterfaceTypeSymbol) => TryGetIEnumerableItemGenericType(typeSymbol, IEnumerableT, out iEnumerableInterfaceTypeSymbol);

    #endregion Public 方法
}
