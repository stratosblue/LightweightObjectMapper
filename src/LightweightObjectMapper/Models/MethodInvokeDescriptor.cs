using System.Collections.Generic;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 方法调用描述
/// </summary>
internal readonly record struct MethodInvokeDescriptor
{
    private readonly string _fullInvokeFormat;

    /// <summary>
    /// 类型名
    /// </summary>
    public string? ClassName { get; }

    /// <summary>
    /// 类型完整名称
    /// </summary>
    public string? FullClassName { get; }

    /// <summary>
    /// 调用方法格式化字符串
    /// </summary>
    public string InvokeFormat { get; }

    /// <summary>
    /// 方法名
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// 附加状态
    /// </summary>
    public IReadOnlyList<object> States { get; }

    /// <inheritdoc cref="MethodInvokeDescriptor"/>
    public MethodInvokeDescriptor(string methodName, string invokeFormat, params object[] states)
    {
        ClassName = null;
        FullClassName = null;
        MethodName = methodName;
        InvokeFormat = invokeFormat;
        States = states;
        _fullInvokeFormat = invokeFormat;
    }

    /// <inheritdoc cref="MethodInvokeDescriptor"/>
    public MethodInvokeDescriptor(string className, string fullClassName, string methodName, string invokeFormat, params object[] states)
    {
        ClassName = className;
        FullClassName = fullClassName;
        MethodName = methodName;
        InvokeFormat = invokeFormat;
        States = states;
        _fullInvokeFormat = $"{FullClassName}.{InvokeFormat}";
    }

    public string FormaAliasInvoke(string aliasClassName, object arg0)
    {
        return string.Format($"{aliasClassName}.{InvokeFormat}", arg0);
    }

    public string FormatAliasInvoke(string aliasClassName, object arg0, object arg1)
    {
        return string.Format($"{aliasClassName}.{InvokeFormat}", arg0, arg1);
    }

    public string FormatAliasInvoke(string aliasClassName, object arg0, object arg1, object arg2)
    {
        return string.Format($"{aliasClassName}.{InvokeFormat}", arg0, arg1, arg2);
    }

    public string FormatFullInvoke(object arg0)
    {
        if (!string.IsNullOrWhiteSpace(FullClassName))
        {
            return string.Format(_fullInvokeFormat, arg0);
        }
        return string.Format(InvokeFormat, arg0);
    }

    public string FormatFullInvoke(object arg0, object arg1)
    {
        if (!string.IsNullOrWhiteSpace(FullClassName))
        {
            return string.Format(_fullInvokeFormat, arg0, arg1);
        }
        return string.Format(InvokeFormat, arg0, arg1);
    }

    public string FormatFullInvoke(object arg0, object arg1, object arg2)
    {
        if (!string.IsNullOrWhiteSpace(FullClassName))
        {
            return string.Format(_fullInvokeFormat, arg0, arg1, arg2);
        }
        return string.Format(InvokeFormat, arg0, arg1, arg2);
    }

    public override string ToString()
    {
        return _fullInvokeFormat ?? InvokeFormat ?? "null";
    }
}
