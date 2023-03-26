using System.Text;

namespace LightweightObjectMapper.Util;

internal static class ConvertUtil
{
    public static string ToHexString(this byte[] data)
    {
        var builder = new StringBuilder(data.Length * 2);

        foreach (var t in data)
        {
            builder.Append(t.ToString("X2"));
        }

        return builder.ToString();
    }
}
