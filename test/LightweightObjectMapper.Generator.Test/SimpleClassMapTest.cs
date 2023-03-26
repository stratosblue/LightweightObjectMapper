namespace LightweightObjectMapper.Test;

[TestClass]
public class SimpleClassMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("SimpleClassMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法
}
