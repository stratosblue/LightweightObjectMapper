using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class ValueTypeToStringMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new ValueTypeToStringClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = (decimal)Random.NextDouble(),
            Property4 = Random.Next() % 2 == 0,
            Property5 = Random.Next(),
        };

        var b = a.MapTo<ValueTypeToStringClass2>();
        AssertEquals(a, b);

        b = new();
        a.MapTo(b);
        AssertEquals(a, b);

        var c = b.MapTo<ValueTypeToStringClass2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(ValueTypeToStringClass1 a, ValueTypeToStringClass2 b)
    {
        Assert.AreEqual(a.Property1.ToString(), b.Property1);
        Assert.AreEqual(a.Property2.ToString(), b.Property2);
        Assert.AreEqual(a.Property3.ToString(), b.Property3);
        Assert.AreEqual(a.Property4.ToString(), b.Property4);
        Assert.AreEqual(a.Property5.ToString(), b.Property5);
        Assert.AreEqual(a.Property6.ToString(), b.Property6);
    }

    #endregion Private 方法
}
