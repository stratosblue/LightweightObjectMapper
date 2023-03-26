using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class SimpleClassArrayMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = RandomArray(_ => new SimpleClassArrayMapClass1()
        {
            Property1 = Random.Next(),
            Property2 = Random.Next(),
            Property3 = Random.Next().ToString(),
            Property4 = Random.Next() % 2 == 0,
            Property5 = Random.Next(),
        }).ToList();

        var b = a.MapTo<SimpleClassArrayMapClass2[]>();
        AssertEquals(a, b);

        var c = b.MapTo<SimpleClassArrayMapClass2[]>();
        AssertEquals(a, c);

        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo<SimpleClassArrayMapClass2[]>());
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(IEnumerable<SimpleClassArrayMapClass1> a, IEnumerable<SimpleClassArrayMapClass2> b)
    {
        Assert.IsFalse(ReferenceEquals(a, b));

        CollectionEquals(a, b, (l, r) =>
        {
            return EqualityComparer<int>.Default.Equals(l.Property1, r.Property1)
                   && EqualityComparer<double>.Default.Equals(l.Property2, r.Property2)
                   && EqualityComparer<string>.Default.Equals(l.Property3, r.Property3)
                   && EqualityComparer<bool>.Default.Equals(l.Property4, r.Property4)
                   && EqualityComparer<int?>.Default.Equals(l.Property5, r.Property5)
                   && EqualityComparer<int?>.Default.Equals(l.Property6, r.Property6);
        });
    }

    #endregion Private 方法
}
