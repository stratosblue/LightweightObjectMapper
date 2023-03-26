#if !NET472_OR_GREATER && !NETSTANDARD2_0 && !NETSTANDARD2_1

using LightweightObjectMapper;

namespace MapperTestLibrary;

internal class InitPropertyMapClass1
{
    public int Property1 { get; set; }

    public double Property2 { get; set; }

    public string Property3 { get; set; }

    public bool Property4 { get; set; }

    public int? Property5 { get; set; }

    public int? Property6 { get; set; }
}

internal class InitPropertyMapClass2
{
    public int Property1 { get; init; }

    public double Property2 { get; init; }

    public string Property3 { get; init; }

    public bool Property4 { get; init; }

    public int? Property5 { get; init; }

    public int? Property6 { get; init; }
}

file class Extensions
{
    public Extensions()
    {
        new InitPropertyMapClass1().MapTo<InitPropertyMapClass2>();
        new InitPropertyMapClass2().MapTo<InitPropertyMapClass2>();
    }
}

#endif
