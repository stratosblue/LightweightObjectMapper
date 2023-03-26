using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightweightObjectMapper.Util;

/// <summary>
/// 已访问的成员收集器
/// </summary>
internal class AccessedMembersCollector : CSharpSyntaxWalker
{
    #region Private 字段

    private readonly SyntaxToken _identifier;

    #endregion Private 字段

    #region Public 属性

    public List<string> AccessedMemberNames { get; } = new();

    #endregion Public 属性

    #region Public 构造函数

    public AccessedMembersCollector(SyntaxToken identifier)
    {
        _identifier = identifier;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        if (node.Parent is not InvocationExpressionSyntax
            && node.Expression is IdentifierNameSyntax identifierNameSyntax
            && string.Equals(identifierNameSyntax.Identifier.Text, _identifier.Text, StringComparison.Ordinal))
        {
            AccessedMemberNames.Add(node.Name.Identifier.ValueText);
        }
        else if (node.Expression is MemberAccessExpressionSyntax innerMemberAccessExpressionSyntax)
        {
            VisitMemberAccessExpression(innerMemberAccessExpressionSyntax);
        }
    }

    #endregion Public 方法
}
