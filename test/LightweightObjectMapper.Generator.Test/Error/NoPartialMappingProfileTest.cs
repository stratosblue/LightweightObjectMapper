namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class NoPartialMappingProfileTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "NoPartialMappingProfile.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 1, DiagnosticDescriptors.Error.NeedPartialKeyword.Id);
    }

    #endregion Public 方法
}
