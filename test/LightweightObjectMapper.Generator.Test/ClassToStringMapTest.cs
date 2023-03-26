namespace LightweightObjectMapper.Test;

[TestClass]
public class ClassToStringMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("ClassToStringMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法
}
