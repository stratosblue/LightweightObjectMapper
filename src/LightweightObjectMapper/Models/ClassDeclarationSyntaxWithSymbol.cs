using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightweightObjectMapper.Models;

[DebuggerDisplay("{Symbol}")]
internal struct ClassDeclarationSyntaxWithSymbol
{
    #region Public 属性

    public Compilation? Compilation { get; set; }

    public INamedTypeSymbol Symbol { get; }

    public ClassDeclarationSyntax Syntax { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ClassDeclarationSyntaxWithSymbol(ClassDeclarationSyntax syntax, INamedTypeSymbol symbol)
    {
        Syntax = syntax;
        Symbol = symbol;
    }

    #endregion Public 构造函数
}
