using Microsoft.CodeAnalysis.Text;

namespace LightweightObjectMapper.Models;

internal struct SourceWithHint
{
    #region Public 属性

    public string HintName { get; }

    public SourceText Source { get; }

    #endregion Public 属性

    #region Public 构造函数

    public SourceWithHint(string hintName, SourceText source)
    {
        HintName = hintName;
        Source = source;
    }

    #endregion Public 构造函数
}
