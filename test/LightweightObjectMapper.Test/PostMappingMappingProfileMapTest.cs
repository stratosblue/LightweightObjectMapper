using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class PostMappingMappingProfileMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new PostMappingMappingProfileMapClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = true,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<PostMappingMappingProfileMapClass2>();
        AssertEquals(a, b);
        Assert.AreEqual(a.Property1, b.Property6);

        var c = b.MapTo<PostMappingMappingProfileMapClass2>();
        AssertEquals(a, c);
        Assert.AreEqual(b.Property1, c.Property6);
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(PostMappingMappingProfileMapClass1 a, PostMappingMappingProfileMapClass2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        Assert.AreEqual(a.Property2, b.Property2);
        Assert.AreEqual(a.Property3, b.Property3);
        Assert.AreEqual(a.Property4, b.Property4);
        Assert.AreEqual(a.Property5, b.Property5);
    }

    #endregion Private 方法
}
