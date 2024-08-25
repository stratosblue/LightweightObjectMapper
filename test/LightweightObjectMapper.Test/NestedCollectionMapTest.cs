using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class NestedCollectionMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new NestedCollectionMapRecord1(Random.Next(), RandomArray(_ => new InnerNestedCollectionMapRecord1(Random.Next(), Random.Next())).ToList());

        var b = a.MapTo<NestedCollectionMapRecord2>();
        AssertEquals(a, b);

        var c = b.MapTo<NestedCollectionMapRecord2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(NestedCollectionMapRecord1 a, NestedCollectionMapRecord2 b)
    {
        Assert.AreEqual(a.Property1, b.Property1);
        var aItems = a.Items.ToList();
        var bItems = b.Items.ToList();
        Assert.AreEqual(aItems.Count, bItems.Count);

        for (var i = 0; i < aItems.Count; i++)
        {
            var ai = aItems[i];
            var bi = bItems[i];
            Assert.AreEqual(ai.Property1, bi.Property1);
            Assert.AreEqual(ai.Property2, bi.Property2);
        }
    }

    #endregion Private 方法
}
