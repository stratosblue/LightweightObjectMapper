﻿// <Auto-Generated/>

#if !NO_BUILD_ERROR_CODE

using System.Collections.Concurrent;
using System.Collections.Generic;
using LightweightObjectMapper;

namespace MapperTestLibrary;

internal class UndefinedCollectionMappingClass1
{
    public int Property1 { get; set; }

    public double Property2 { get; set; }

    public string Property3 { get; set; }

    public bool Property4 { get; set; }

    public int? Property5 { get; set; }

    public int? Property6 { get; set; }
}

file class Extensions
{
    public Extensions()
    {
        var list = new List<UndefinedCollectionMappingClass1>();

        list.MapTo<ConcurrentBag<UndefinedCollectionMappingClass1>>();
        list.MapTo<ConcurrentQueue<UndefinedCollectionMappingClass1>>();
        list.MapTo<ConcurrentStack<UndefinedCollectionMappingClass1>>();
    }
}

#endif