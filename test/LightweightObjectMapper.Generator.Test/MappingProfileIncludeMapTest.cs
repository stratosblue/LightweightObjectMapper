using MappingProfileProvideLibrary;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Test;

[TestClass]
public class MappingProfileIncludeMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("MappingProfileIncludeMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法

    public override IEnumerable<MetadataReference>? ProvideAppendReferences()
    {
        return LoadFrameworkMetadataReferences().Append(LoadMetadataReferenceByType<InternalClass1>());
    }
}
