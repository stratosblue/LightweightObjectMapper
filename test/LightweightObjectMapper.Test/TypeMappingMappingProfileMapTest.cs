using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class TypeMappingMappingProfileMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new TypeMappingMappingProfileMapClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = true,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<TypeMappingMappingProfileMapClass2>();
        AssertNotEquals(a, b);
        Assert.AreEqual(int.MaxValue, b.Property6);

        var c = b.MapTo<TypeMappingMappingProfileMapClass2>();
        AssertNotEquals(a, c);
        Assert.AreEqual(int.MinValue, c.Property6);
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertNotEquals(TypeMappingMappingProfileMapClass1 a, TypeMappingMappingProfileMapClass2 b)
    {
        Assert.AreNotEqual(a.Property1, b.Property1);
        Assert.AreNotEqual(a.Property2, b.Property2);
        Assert.AreNotEqual(a.Property3, b.Property3);
        Assert.AreNotEqual(a.Property4, b.Property4);
        Assert.AreNotEqual(a.Property5, b.Property5);
        Assert.AreNotEqual(a.Property6, b.Property6);
    }

    #endregion Private 方法
}
