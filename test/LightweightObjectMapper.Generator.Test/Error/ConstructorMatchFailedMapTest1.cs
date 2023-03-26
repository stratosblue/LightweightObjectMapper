namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class ConstructorMatchFailedMapTest1 : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "ConstructorMatchFailedMap1.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 2, DiagnosticDescriptors.Error.ConstructorMatchFailed.Id);
    }

    #endregion Public 方法
}
