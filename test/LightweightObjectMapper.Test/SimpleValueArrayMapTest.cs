using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class SimpleValueArrayMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new SimpleValueArrayClass1()
        {
            Property1 = RandomArray(_ => Random.Next()),
            Property2 = RandomArray(_ => Random.NextDouble()),
            Property3 = RandomArray(_ => Random.Next().ToString()),
            Property4 = RandomArray(_ => Random.Next() % 2 == 0),
            Property5 = RandomArray(_ => new int?(Random.Next())),
        };

        var b = a.MapTo<SimpleValueArrayClass2>();
        AssertEquals(a, b);

        b = new();
        a.MapTo(b);
        AssertEquals(a, b);

        var c = b.MapTo<SimpleValueArrayClass2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(SimpleValueArrayClass1 a, SimpleValueArrayClass2 b)
    {
        Assert.IsFalse(ReferenceEquals(a.Property1, b.Property1));
        Assert.IsFalse(ReferenceEquals(a.Property2, b.Property2));
        Assert.IsFalse(ReferenceEquals(a.Property3, b.Property3));
        Assert.IsFalse(ReferenceEquals(a.Property4, b.Property4));
        Assert.IsFalse(ReferenceEquals(a.Property5, b.Property5));
        Assert.IsNull(a.Property6);
        Assert.IsNull(b.Property6);

        CollectionEquals(a.Property1, b.Property1);
        CollectionEquals(a.Property2, b.Property2);
        CollectionEquals(a.Property3, b.Property3);
        CollectionEquals(a.Property4, b.Property4);
        CollectionEquals(a.Property5, b.Property5);
    }

    #endregion Private 方法
}
