using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace LightweightObjectMapper.Util;

internal static class CodeFormatUtil
{
    #region Public 方法

    public static SourceText Format(string text, CancellationToken cancellationToken = default)
    {
        var codeSyntaxTree = CSharpSyntaxTree.ParseText(text: text, options: new CSharpParseOptions(LanguageVersion.CSharp8), encoding: Encoding.UTF8, cancellationToken: cancellationToken);

        var sourceText = codeSyntaxTree.GetRoot(cancellationToken).NormalizeWhitespace().SyntaxTree.GetText(cancellationToken);

        return sourceText;
    }

    #endregion Public 方法
}
