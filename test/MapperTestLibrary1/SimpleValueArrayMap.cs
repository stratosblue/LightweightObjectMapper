﻿// <Auto-Generated/>
using LightweightObjectMapper;

namespace MapperTestLibrary;

internal class SimpleValueArrayClass1
{
    public int[] Property1 { get; set; }

    public double[] Property2 { get; set; }

    public string[] Property3 { get; set; }

    public bool[] Property4 { get; set; }

    public int?[] Property5 { get; set; }

    public int?[] Property6 { get; set; }
}

internal class SimpleValueArrayClass2
{
    public int[] Property1 { get; set; }

    public double[] Property2 { get; set; }

    public string[] Property3 { get; set; }

    public bool[] Property4 { get; set; }

    public int?[] Property5 { get; set; }

    public int?[] Property6 { get; set; }
}

file class Extensions
{
    public Extensions()
    {
        new SimpleValueArrayClass1().MapTo<SimpleValueArrayClass2>();
        new SimpleValueArrayClass1().MapTo(new SimpleValueArrayClass2());
        new SimpleValueArrayClass2().MapTo<SimpleValueArrayClass2>();
    }
}