#if NET5_0_OR_GREATER

using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class InitPropertyMapTest : ObjectMapTestBase
{
#region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new InitPropertyMapClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = true,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<InitPropertyMapClass2>();
        AssertEquals(a, b);

        var c = b.MapTo<InitPropertyMapClass2>();
        AssertEquals(a, c);

        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo<InitPropertyMapClass2>());
    }

#endregion Public 方法

#region Private 方法

    private static void AssertEquals(InitPropertyMapClass1 a, InitPropertyMapClass2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        Assert.AreEqual(a.Property2, b.Property2);
        Assert.AreEqual(a.Property3, b.Property3);
        Assert.AreEqual(a.Property4, b.Property4);
        Assert.AreEqual(a.Property5, b.Property5);
        Assert.AreEqual(a.Property6, b.Property6);
    }

    private static void AssertNotEquals(InitPropertyMapClass1 a, InitPropertyMapClass2 b)
    {
        Assert.AreNotEqual(a.Property1, b.Property1);
        Assert.AreNotEqual(a.Property2, b.Property2);
        Assert.AreNotEqual(a.Property3, b.Property3);
        Assert.AreNotEqual(a.Property4, b.Property4);
        Assert.AreNotEqual(a.Property5, b.Property5);
        Assert.AreEqual(a.Property6, b.Property6);
    }

#endregion Private 方法
}

#endif
