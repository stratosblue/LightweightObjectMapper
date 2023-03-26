namespace LightweightObjectMapper.Test;

[TestClass]
public class SimpleValueListMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("SimpleValueListMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法
}
