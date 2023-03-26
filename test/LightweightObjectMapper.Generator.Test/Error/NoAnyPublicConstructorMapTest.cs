namespace LightweightObjectMapper.Test.Error;

[TestClass]
public class NoAnyPublicConstructorMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Fail()
    {
        var compilationResult = CompilationEmbeddedSourceWithPath("Error/", "NoAnyPublicConstructorMap.cs");
        NoCompilationWarning(compilationResult);

        CheckDiagnosticsIsSame(compilationResult.GetDiagnosticErrors(), 2, DiagnosticDescriptors.Error.NoAnyPublicConstructor.Id);
    }

    #endregion Public 方法
}
