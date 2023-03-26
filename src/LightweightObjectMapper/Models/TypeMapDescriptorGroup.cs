using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Models;

[DebuggerDisplay("{SourceType} - [{Descriptors.Count}]")]
internal class TypeMapDescriptorGroup
{
    #region Public 属性

    public HashSet<TypeMapDescriptor> Descriptors { get; } = new HashSet<TypeMapDescriptor>();

    /// <summary>
    /// 源类型
    /// </summary>
    public ITypeSymbol SourceType { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeMapDescriptorGroup(ITypeSymbol sourceType, IEnumerable<TypeMapDescriptor> descriptors)
    {
        SourceType = sourceType;
        foreach (var item in descriptors)
        {
            Descriptors.Add(item);
        }
    }

    #endregion Public 构造函数
}
