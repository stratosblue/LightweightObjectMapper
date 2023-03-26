namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class ErrorMappingProfileIncludeTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "ErrorMappingProfileInclude.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 2, DiagnosticDescriptors.Error.ErrorMappingProfileInclude.Id);
    }

    #endregion Public 方法
}
