using MapperTestLibrary;

namespace LightweightObjectMapper.Test;

[TestClass]
public class ClassToObjectMapTest : ObjectMapTestBase
{
    #region Public 方法

    [TestMethod]
    public void Should_Equals()
    {
        var a = new ClassToObjectClass1()
        {
            Property1 = new(),
        };

        var b = a.MapTo<ClassToObjectClass2>();
        AssertEquals(a, b);

        b = new();
        a.MapTo(b);
        AssertEquals(a, b);

        var c = b.MapTo<ClassToObjectClass2>();
        AssertEquals(a, c);

        b = null;
        Assert.ThrowsException<ArgumentNullException>(() => a.MapTo(b!));
        a = null;
        Assert.ThrowsException<ArgumentNullException>(() => a!.MapTo(c));
    }

    #endregion Public 方法

    #region Private 方法

    private static void AssertEquals(ClassToObjectClass1 a, ClassToObjectClass2 b)
    {
        Assert.AreSame(a.Property1, b.Property1);
    }

    #endregion Private 方法
}
