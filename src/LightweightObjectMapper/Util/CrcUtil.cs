using System.Text;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Util;

internal static class CrcUtil
{
    public static string Hash(ITypeSymbol typeSymbol1, ITypeSymbol typeSymbol2)
    {
        return Hash64($"{typeSymbol1.ToFullCodeString()}-{typeSymbol2.ToFullCodeString()}");
    }

    public static string Hash64(string value)
    {
        return Crc64.Hash(Encoding.UTF8.GetBytes(value)).ToHexString();
    }

    public static string ToCrc64String(this string value)
    {
        return Hash64(value);
    }
}
