﻿// <Auto-Generated/>

#if !NO_BUILD_ERROR_CODE

using LightweightObjectMapper;

namespace MapperTestLibrary;

internal class NoAnyPublicConstructorMapClass1
{
}

internal class NoAnyPublicConstructorMapClass2
{
    private NoAnyPublicConstructorMapClass2()
    {
    }
}

file class Extensions
{
    public Extensions()
    {
        new NoAnyPublicConstructorMapClass1().MapTo<NoAnyPublicConstructorMapClass2>();
        new NoAnyPublicConstructorMapClass2().MapTo<NoAnyPublicConstructorMapClass2>();
    }
}

#endif