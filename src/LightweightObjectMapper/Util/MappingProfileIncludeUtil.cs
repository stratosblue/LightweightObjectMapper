using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Util;

internal static class MappingProfileIncludeUtil
{
    #region Public 方法

    public static IEnumerable<INamedTypeSymbol> FindAllIncludeMappingProfiles(INamedTypeSymbol typeSymbol, BuildContext context)
    {
        var includeMappingProfiles = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        FillProfileSet(typeSymbol, context, includeMappingProfiles);

        return includeMappingProfiles;
    }

    public static IEnumerable<INamedTypeSymbol> GetIncludeProfiles(INamedTypeSymbol typeSymbol, BuildContext context)
    {
        var allConstructorArguments = typeSymbol.GetAttributes()
                                                .Where(m => context.SelfTypes.MappingProfileIncludeAttribute.EqualsDefault(m.AttributeClass))
                                                .SelectMany(m => m.ConstructorArguments)
                                                .ToList();
        var list = new List<INamedTypeSymbol>();

        foreach (var typedConstant in allConstructorArguments)
        {
            if (typedConstant.Type is IArrayTypeSymbol)
            {
                list.AddRange(typedConstant.Values.Select(m => m.Value).OfType<INamedTypeSymbol>());
            }
            else if (typedConstant.Value is INamedTypeSymbol namedTypeSymbol)
            {
                list.Add(namedTypeSymbol);
            }
        }

        return list;
    }

    #endregion Public 方法

    #region Private 方法

    private static void FillProfileSet(INamedTypeSymbol typeSymbol, BuildContext context, HashSet<INamedTypeSymbol> includeMappingProfiles)
    {
        if (includeMappingProfiles.Contains(typeSymbol))
        {
            return;
        }
        includeMappingProfiles.Add(typeSymbol);

        foreach (var includeTypeSymbol in GetIncludeProfiles(typeSymbol, context))
        {
            FillProfileSet(includeTypeSymbol, context, includeMappingProfiles);
        }
    }

    #endregion Private 方法
}
