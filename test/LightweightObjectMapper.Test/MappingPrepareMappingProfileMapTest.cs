using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class MappingPrepareMappingProfileMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new MappingPrepareMappingProfileMapClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = true,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<MappingPrepareMappingProfileMapClass2>();
        AssertEquals(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

        var c = b.MapTo<MappingPrepareMappingProfileMapClass2>();
        AssertEquals(a, c);
        Assert.AreEqual(b.GetHashCode(), c.GetHashCode());
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(MappingPrepareMappingProfileMapClass1 a, MappingPrepareMappingProfileMapClass2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        Assert.AreEqual(a.Property2, b.Property2);
        Assert.AreEqual(a.Property3, b.Property3);
        Assert.AreEqual(a.Property4, b.Property4);
        Assert.AreEqual(a.Property5, b.Property5);
        Assert.AreEqual(a.Property6, b.Property6);
    }

    #endregion Private 方法
}
