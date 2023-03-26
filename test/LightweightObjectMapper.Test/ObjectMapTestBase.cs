namespace LightweightObjectMapper.Test;

public abstract class ObjectMapTestBase
{
    #region Protected 属性

    protected Random Random { get; } = new Random();

    #endregion Protected 属性

    #region Protected 方法

    protected static void CollectionEquals<TItem>(IEnumerable<TItem> a, IEnumerable<TItem> b, IEqualityComparer<TItem>? comparer = null)
    {
        Assert.AreEqual(a.Count(), b.Count());

        var enumeratora = a.GetEnumerator();
        var enumeratorb = b.GetEnumerator();

        comparer ??= EqualityComparer<TItem>.Default;

        var index = 0;
        while (enumeratora.MoveNext() && enumeratorb.MoveNext())
        {
            Assert.AreEqual(enumeratora.Current, enumeratorb.Current, comparer, $"not equal at {index++}.");
        }
    }

    protected static void CollectionEquals<TItem1, TItem2>(IEnumerable<TItem1> a, IEnumerable<TItem2> b, Func<TItem1, TItem2, bool> comparer)
    {
        Assert.AreEqual(a.Count(), b.Count());

        var enumeratora = a.GetEnumerator();
        var enumeratorb = b.GetEnumerator();

        var index = 0;
        while (enumeratora.MoveNext() && enumeratorb.MoveNext())
        {
            Assert.IsTrue(comparer(enumeratora.Current, enumeratorb.Current), $"not equal at {index++}.");
        }
    }

    protected T[] RandomArray<T>(Func<int, T> itemFactory, int maxCount = 20)
    {
        return Enumerable.Range(0, Random.Next(1, maxCount)).Select(itemFactory).ToArray();
    }

    #endregion Protected 方法
}
