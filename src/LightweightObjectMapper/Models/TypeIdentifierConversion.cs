using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

/// <summary>
/// 类型标识符转换信息
/// </summary>
[DebuggerDisplay("{Conversion} - {Identifier}")]
internal struct TypeIdentifierConversion
{
    #region Public 属性

    /// <summary>
    /// 是否可转换
    /// </summary>
    public bool CanConversion => Conversion && Identifier is not null;

    /// <summary>
    /// 类型转换信息
    /// </summary>
    public TypeConversion Conversion { get; }

    /// <summary>
    /// <see cref="ReportDiagnosticDescriptor"/> 的消息参数
    /// </summary>
    public object?[]? DiagnosticMessageArgs { get; private set; }

    /// <summary>
    /// 标识符
    /// </summary>
    public TypeReadWriteIdentifier? Identifier { get; }

    /// <summary>
    /// 用于报告IDE的信息
    /// </summary>
    public DiagnosticDescriptor? ReportDiagnosticDescriptor { get; private set; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeIdentifierConversion()
    {
        Conversion = default;
        Identifier = null;
        DiagnosticMessageArgs = null;
        ReportDiagnosticDescriptor = null;
    }

    public TypeIdentifierConversion(DiagnosticDescriptor reportDiagnosticDescriptor, object?[]? diagnosticMessageArgs = null) : this()
    {
        ReportDiagnosticDescriptor = reportDiagnosticDescriptor;
        DiagnosticMessageArgs = diagnosticMessageArgs;
    }

    public TypeIdentifierConversion(TypeReadWriteIdentifier identifier, TypeConversion conversion) : this()
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Conversion = conversion;
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator bool(TypeIdentifierConversion value)
    {
        return value.CanConversion;
    }

    #endregion Public 方法
}
