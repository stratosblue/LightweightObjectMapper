namespace LightweightObjectMapper.Test.Warning;

[TestClass]
public class WarningInitPropertyMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Warning/", "WarningInitPropertyMap.cs");
        NoCompilationError(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticWarnings(), 1, DiagnosticDescriptors.Warning.InitOnlyPropertyCanNotMap.Id);
    }

    #endregion Public 方法
}
