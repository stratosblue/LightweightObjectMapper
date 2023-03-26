using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

internal record class CompilationProperties
{
    /// <summary>
    /// 映射方法的可访问性
    /// </summary>
    public Accessibility MappingMethodAccessibility { get; set; } = Accessibility.Internal;
}
