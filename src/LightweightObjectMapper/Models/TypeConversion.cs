using System.Diagnostics;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型转换信息
/// </summary>
[DebuggerDisplay("{CanConversion} {ConversionInvokeDescriptor}")]
internal readonly record struct TypeConversion
{
    #region Public 属性

    public bool CanConversion { get; }

    public MethodInvokeDescriptor? ConversionInvokeDescriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeConversion(bool canConversion) : this()
    {
        CanConversion = canConversion;
    }

    public TypeConversion(MethodInvokeDescriptor? conversionInvokeDescriptor)
    {
        CanConversion = true;
        ConversionInvokeDescriptor = conversionInvokeDescriptor;
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator bool(TypeConversion value)
    {
        return value.CanConversion;
    }

    public string GetConversionExpression(string variableName, string? aliasClassName = null)
    {
        if (ConversionInvokeDescriptor is null)
        {
            return variableName;
        }
        if (string.IsNullOrWhiteSpace(aliasClassName))
        {
            return ConversionInvokeDescriptor.Value.FormatFullInvoke(variableName);
        }
        return ConversionInvokeDescriptor.Value.FormaAliasInvoke(aliasClassName!, variableName);
    }

    #endregion Public 方法
}
