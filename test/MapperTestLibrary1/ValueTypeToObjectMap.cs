﻿// <Auto-Generated/>
using LightweightObjectMapper;

namespace MapperTestLibrary;

internal class ValueTypeToObjectClass1
{
    public int Property1 { get; set; }

    public double Property2 { get; set; }

    public decimal Property3 { get; set; }

    public bool Property4 { get; set; }

    public int? Property5 { get; set; }

    public int? Property6 { get; set; }
}

internal class ValueTypeToObjectClass2
{
    public object Property1 { get; set; }

    public object Property2 { get; set; }

    public object Property3 { get; set; }

    public object Property4 { get; set; }

    public object Property5 { get; set; }

    public object Property6 { get; set; }
}

file class Extensions
{
    public Extensions()
    {
        new ValueTypeToObjectClass1().MapTo<ValueTypeToObjectClass2>();
        new ValueTypeToObjectClass1().MapTo(new ValueTypeToObjectClass2());
        new ValueTypeToObjectClass2().MapTo<ValueTypeToObjectClass2>();
    }
}