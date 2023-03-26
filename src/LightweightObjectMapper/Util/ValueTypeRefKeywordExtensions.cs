using LightweightObjectMapper.Models;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

internal static class ValueTypeRefKeywordExtensions
{
    #region Public 方法

    /// <summary>
    /// 如果值类型则返回 "ref"
    /// </summary>
    /// <param name="typeMapMetaData"></param>
    /// <returns></returns>
    public static string GetParameterRefKeyword(this TypeMapMetaData typeMapMetaData) => ReturnValue(typeMapMetaData.IsValueType);

    /// <summary>
    /// 如果值类型则返回 "ref"
    /// </summary>
    /// <param name="parameterSymbol"></param>
    /// <returns></returns>
    public static string GetParameterRefKeyword(this IParameterSymbol parameterSymbol) => ReturnValue(parameterSymbol.Type.IsValueType);

    #endregion Public 方法

    #region Private 方法

    private static string ReturnValue(bool value)
    {
        return value ? "ref" : string.Empty;
    }

    #endregion Private 方法
}
