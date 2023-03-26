using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

[DebuggerDisplay("{Identifier}({TypeSymbol})")]
internal class TypeReadWriteIdentifier : IEquatable<TypeReadWriteIdentifier>
{
    #region Public 属性

    /// <summary>
    /// 标识符
    /// </summary>
    public string Identifier { get; }

    public bool IsInitOnly { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public SymbolKind Kind { get; }

    public bool Readable { get; }

    /// <summary>
    /// 符号
    /// </summary>
    public ISymbol Symbol { get; }

    public ITypeSymbol TypeSymbol { get; }

    public bool Writeable { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeReadWriteIdentifier(string identifier, ISymbol symbol, SymbolKind kind, ITypeSymbol typeSymbol, bool readable, bool writeable)
    {
        Identifier = identifier;
        Symbol = symbol;
        Kind = kind;
        TypeSymbol = typeSymbol;
        Readable = readable;
        Writeable = writeable;
        IsInitOnly = (Symbol as IPropertySymbol)?.SetMethod?.IsInitOnly ?? false;
    }

    #endregion Public 构造函数

    #region Public 方法

    public bool Equals(TypeReadWriteIdentifier other)
    {
        return string.Equals(Identifier, other.Identifier, StringComparison.Ordinal)
               && Symbol.EqualsDefault(other.Symbol)
               && TypeSymbol.EqualsDefault(other.TypeSymbol);
    }

    public override int GetHashCode()
    {
        int hashCode = -1084615088;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(Symbol);
        hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(TypeSymbol);
        return hashCode;
    }

    #endregion Public 方法
}
