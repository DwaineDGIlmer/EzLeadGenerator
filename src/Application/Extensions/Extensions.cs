using Core.Extensions;
using Core.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c599f69c3dd3ec398aa236557324d13db0f01fe1619e95bd66ab4fbd53143b2e57470a9c156080f2e3b088da0a7d40ce549ed7d803bc7cfc904077dce8ea5262c4afc77594841fb916db84485db81dfa6ba6cba1d449c0cb8c6aafd42245221dc310fae03f9b18c258c7939cd293f01a9b1ab9c433a53b278022f02a46958797")]
namespace Application.Extensions;

/// <summary>
/// Provides extension methods for various utility operations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Determines whether the specified object is null.
    /// </summary>
    /// <param name="obj">The object to check for null.</param>
    /// <param name="length"></param>
    /// <returns><c>true</c> if the object is null; otherwise, <c>false</c>.</returns>
    public static string FileSystemName([NotNullWhen(false)] this string obj, int length = 64)
    {
        if (string.IsNullOrEmpty(obj))
        {
            return string.Empty;
        }
        return FileSystemHelpers.SanitizeForFileSystem(obj.GenHashString())[..length];
    }
}
