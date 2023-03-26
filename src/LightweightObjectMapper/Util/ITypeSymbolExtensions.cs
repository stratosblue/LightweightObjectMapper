using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

internal static class ITypeSymbolExtensions
{
    #region Public 方法

    /// <summary>
    /// 获取所有成员，包括其父类的成员
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static ImmutableArray<ISymbol> GetAllMembers(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
        {
            return ImmutableArray<ISymbol>.Empty;
        }
        var symbols = new List<ImmutableArray<ISymbol>>();
        do
        {
            symbols.Add(typeSymbol.GetMembers());
            typeSymbol = typeSymbol.BaseType;
        }
        while (typeSymbol != null);
        return symbols.SelectMany(m => m).ToImmutableArray();
    }

    #endregion Public 方法
}
