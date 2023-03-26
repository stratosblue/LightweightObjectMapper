using MappingProfileProvideLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class MappingProfileIncludeMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new InternalClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = Random.Next() % 2 == 0,
            Property5 = Random.Next(),
            Property6 = Random.Next(),
        };

        var b = a.MapTo<InternalClass2>();
        AssertEquals(a, b);

        var c = b.MapTo<InternalClass2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(InternalClass1 a, InternalClass2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        Assert.AreEqual(a.Property2, b.Property2);
        Assert.AreEqual(a.Property3, b.Property3);
        Assert.AreEqual(a.Property4, b.Property4);
        Assert.AreEqual(a.Property5, b.Property5);
        Assert.AreNotEqual(a.Property6, b.Property6);
    }

    #endregion Private 方法
}
