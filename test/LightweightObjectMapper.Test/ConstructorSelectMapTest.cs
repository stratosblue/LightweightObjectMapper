using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class ConstructorSelectMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new ConstructorSelectMapFullClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = Random.Next() % 2 == 0,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<ConstructorSelectMapFullClass2>();
        AssertEquals(a, b);

        var c = new ConstructorSelectMapClass1()
        {
            Property1 = Random.Next(),
        };

        b = c.MapTo<ConstructorSelectMapFullClass2>();
        Assert.AreEqual(c.Property1, b.Property1);

        var d = new ConstructorSelectMapClass2()
        {
            Property2 = Random.Next(),
        };

        b = d.MapTo<ConstructorSelectMapFullClass2>();
        Assert.AreEqual(d.Property2, b.Property2);

        var e = new ConstructorSelectMapClass3()
        {
            Property3 = Random.Next().ToString(),
        };

        b = e.MapTo<ConstructorSelectMapFullClass2>();
        Assert.AreEqual(e.Property3, b.Property3);
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(ConstructorSelectMapFullClass1 a, ConstructorSelectMapFullClass2 b)
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
