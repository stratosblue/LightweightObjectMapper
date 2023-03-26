using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class ClassToStringMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new ClassToStringClass1()
        {
            Property1 = new()
            {
                Value = Random.Next()
            },
        };

        var b = a.MapTo<ClassToStringClass2>();
        AssertEquals(a, b);

        b = new();
        a.MapTo(b);
        AssertEquals(a, b);

        var c = b.MapTo<ClassToStringClass2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(ClassToStringClass1 a, ClassToStringClass2 b)
    {
        Assert.AreEqual(a.Property1.ToString(), b.Property1);
    }

    #endregion Private 方法
}
