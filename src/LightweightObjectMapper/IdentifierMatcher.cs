using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using LightweightObjectMapper.Models;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

/// <summary>
/// 名称匹配打分委托
/// </summary>
/// <param name="sourceName"></param>
/// <param name="matchName"></param>
/// <returns></returns>
internal delegate int NameMatchingScoreDelegate(string sourceName, string matchName);

/// <summary>
/// 标识符匹配器
/// </summary>
internal class IdentifierMatcher
{
    #region Private 字段

    private readonly TypeConversionMatcher _typeConversionMatcher;

    #endregion Private 字段

    #region Public 构造函数

    public IdentifierMatcher(TypeConversionMatcher typeConversionMatcher)
    {
        _typeConversionMatcher = typeConversionMatcher ?? throw new ArgumentNullException(nameof(typeConversionMatcher));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 匹配字段/属性
    /// </summary>
    /// <param name="identifier">要进行匹配的字段/属性</param>
    /// <param name="searchDictionary">搜索字典</param>
    /// <param name="nameMatchingScoreDelegate"></param>
    /// <returns></returns>
    public TypeIdentifierConversion Match(TypeReadWriteIdentifier identifier, ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> searchDictionary, NameMatchingScoreDelegate? nameMatchingScoreDelegate = null)
    {
        if (searchDictionary.TryGetValue(identifier.Identifier, out var identifiers))
        {
            //查找名称全匹配的字段/属性
            if (identifiers.FirstOrDefault(m => string.Equals(identifier.Identifier, m.Identifier, StringComparison.Ordinal)) is TypeReadWriteIdentifier targetIdentifier
                && _typeConversionMatcher.GetTypeConversion(targetIdentifier.TypeSymbol, identifier.TypeSymbol) is TypeConversion typeConversion
                && typeConversion)
            {
                return new(targetIdentifier, typeConversion);
            }

            nameMatchingScoreDelegate ??= DefaultNameMatchingScore;

            //遍历所有标识符
            foreach (var matchIdentifier in identifiers.OrderByDescending(m => nameMatchingScoreDelegate(identifier.Identifier, m.Identifier)))
            {
                if (_typeConversionMatcher.GetTypeConversion(matchIdentifier.TypeSymbol, identifier.TypeSymbol) is TypeConversion matchTypeConversion
                   && matchTypeConversion)
                {
                    return new(matchIdentifier, matchTypeConversion);
                }
            }
            var diagnosticMessageArgs = new object?[] { identifier.Identifier, string.Join(", ", identifiers.Select(m => $"{m.Identifier}({m.TypeSymbol})")), $"{identifier.Identifier}({identifier.TypeSymbol})" };
            return new TypeIdentifierConversion(DiagnosticDescriptors.Warning.PropertyOrFieldTypeNotMatch, diagnosticMessageArgs);
        }
        return new();
    }

    /// <summary>
    /// 匹配构造函数
    /// </summary>
    /// <param name="orderedConstructors">必须是参数由少到多的构造函数列表</param>
    /// <param name="searchDictionary">搜索字典</param>
    /// <returns></returns>
    public ConstructorMatchResult MatchConstructor(IEnumerable<TypeConstructorDescriptor> orderedConstructors, ImmutableDictionary<string, ImmutableArray<TypeReadWriteIdentifier>> searchDictionary)
    {
        if (!orderedConstructors.Any())
        {
            return default;
        }
        if (orderedConstructors.First() is TypeConstructorDescriptor firstConstructor
            && firstConstructor.OrderedParameters.Length == 0)
        {
            // 无参构造函数
            return new(true, firstConstructor, ImmutableArray<TypeIdentifierConversion>.Empty);
        }
        foreach (var constructor in orderedConstructors)
        {
            // 构造函数参数匹配跳过首字符
            var conversions = constructor.OrderedParameters.Select(m => Match(m, searchDictionary, static (sourceName, matchName) => InternalNameMatchingScore(sourceName, matchName, 1))).ToArray();
            if (conversions.All(m => m))
            {
                return new(true, constructor, conversions.ToImmutableArray());
            }
        }

        return default;
    }

    #endregion Public 方法

    #region Static

    /// <summary>
    /// 默认名称匹配度打分
    /// </summary>
    /// <param name="sourceName"></param>
    /// <param name="matchName"></param>
    /// <returns></returns>
    public static int DefaultNameMatchingScore(string sourceName, string matchName) => InternalNameMatchingScore(sourceName, matchName, 0);

    internal static int InternalNameMatchingScore(string sourceName, string matchName, int startIndex = 0)
    {
        var score = 0;
        var length = sourceName.Length;
        for (int i = startIndex; i < length; i++)
        {
            if (sourceName[i] == matchName[i])
            {
                score += length - i;
            }
        }
        return score;
    }

    #endregion Static
}
