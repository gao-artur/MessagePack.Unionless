using System.Text;
// ReSharper disable All

// https://github.com/neuecc/MessagePack-CSharp/blob/v2.5.108/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/StringEncoding.cs

namespace MessagePack.Unionless.Internal;

internal static class StringEncoding
{
    internal static readonly Encoding UTF8 = new UTF8Encoding(false);
}