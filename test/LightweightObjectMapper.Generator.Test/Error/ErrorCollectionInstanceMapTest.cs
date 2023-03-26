namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class ErrorCollectionInstanceMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "ErrorCollectionInstanceMap.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 1, DiagnosticDescriptors.Error.CollectionMapIncorrectUsage.Id);
    }

    #endregion Public 方法
}
