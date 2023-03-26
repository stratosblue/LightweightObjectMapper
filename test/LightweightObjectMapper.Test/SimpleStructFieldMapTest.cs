using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class SimpleStructFieldMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new SimpleStructFieldMapStruct1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = Random.Next() % 2 == 0,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<SimpleStructFieldMapStruct2>();
        AssertEquals(a, b);

        b = new();
        a.MapTo(ref b);
        AssertEquals(a, b);

        var c = b.MapTo<SimpleStructFieldMapStruct2>();
        AssertEquals(a, c);

        var d = new SimpleStructFieldMapStruct1();
        a.MapTo(ref d);
        AssertEquals(a, d);
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(SimpleStructFieldMapStruct1 a, SimpleStructFieldMapStruct2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        Assert.AreEqual(a.Property2, b.Property2);
        Assert.AreEqual(a.Property3, b.Property3);
        Assert.AreEqual(a.Property4, b.Property4);
        Assert.AreEqual(a.Property5, b.Property5);
        Assert.AreEqual(a.Property6, b.Property6);
    }

    private static void AssertEquals(SimpleStructFieldMapStruct1 a, SimpleStructFieldMapStruct1 b)
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
