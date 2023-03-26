namespace LightweightObjectMapper.Test;

[TestClass]
public class TypeMemberIgnoreMappingMappingProfileMapTest : SourceGeneratorTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_NoWarning()
    {
        var compilationResult = CompilationEmbeddedSource("TypeMemberIgnoreMappingMappingProfileMap.cs");
        NoCompilationError(compilationResult);
        NoCompilationWarning(compilationResult);
    }

    #endregion Public 方法
}
