namespace System.Collections.Generic;

internal static class HashSetExtensions
{
    #region Public 方法

    public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            hashSet.Add(item);
        }
        return hashSet;
    }

    #endregion Public 方法
}
