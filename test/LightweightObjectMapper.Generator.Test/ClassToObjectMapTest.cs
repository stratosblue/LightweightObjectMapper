namespace LightweightObjectMapper.Test;

[TestClass]
public class ClassToObjectMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("ClassToObjectMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法
}
