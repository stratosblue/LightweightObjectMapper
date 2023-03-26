namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class AmbiguousMapTypeMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "AmbiguousMapTypeMap.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 42, DiagnosticDescriptors.Error.AmbiguousMapType.Id);
    }

    #endregion Public 方法
}
