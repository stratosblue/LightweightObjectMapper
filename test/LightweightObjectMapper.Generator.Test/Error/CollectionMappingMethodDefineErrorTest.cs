namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class CollectionMappingMethodDefineErrorTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "CollectionMappingMethodDefineError.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 9, DiagnosticDescriptors.Error.CollectionMappingMethodDefineError.Id);
    }

    #endregion Public 方法
}
