using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class UndefinedCollectionMappingTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "UndefinedCollectionMapping.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 3, DiagnosticDescriptors.Error.UndefinedCollectionMapping.Id);
    }

    #endregion Public 方法

    public override IEnumerable<MetadataReference>? ProvideAppendReferences()
    {
        return new[]
        {
            LoadMetadataReferenceByType<ConcurrentBag<int>>(),
        };
    }
}
