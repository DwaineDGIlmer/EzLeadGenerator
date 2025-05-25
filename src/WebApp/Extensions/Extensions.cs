using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c599f69c3dd3ec398aa236557324d13db0f01fe1619e95bd66ab4fbd53143b2e57470a9c156080f2e3b088da0a7d40ce549ed7d803bc7cfc904077dce8ea5262c4afc77594841fb916db84485db81dfa6ba6cba1d449c0cb8c6aafd42245221dc310fae03f9b18c258c7939cd293f01a9b1ab9c433a53b278022f02a46958797")]
namespace WebApp.Extensions;

/// <summary>
/// Provides extension methods for various utility operations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Determines whether the specified object is null.
    /// </summary>
    /// <typeparam name="T">The type of the object to check.</typeparam>
    /// <param name="obj">The object to check for null.</param>
    /// <returns><c>true</c> if the object is null; otherwise, <c>false</c>.</returns>
    public static bool IsNull<T>([NotNullWhen(false)] this T obj)
    {
        if (obj is string str)
            return string.IsNullOrEmpty(str);

        return obj == null;
    }

    /// <summary>
    /// Ensures that the specified object is not null or, in the case of a string, not empty.  Throws an exception if
    /// the validation fails.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="obj">The object to validate. If the object is a string, it must not be null or empty.</param>
    /// <param name="message">An optional custom error message to include in the exception. If not provided, a default message is used.</param>
    /// <param name="paramName">The name of the parameter being validated. This is automatically captured by the compiler.</param>
    /// <returns>The validated object, if it is not null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null, or if <paramref name="obj"/> is a string and is empty.</exception>
    public static T IsNullThrow<T>(
        [NotNullWhen(false)] this T obj,
        string? message = null,
        [CallerArgumentExpression(nameof(obj))] string? paramName = null)
    {
        bool isNull = false;
        if (obj == null)
        {
            isNull = true;
        }
        else if (obj is string str)
        {
            isNull = string.IsNullOrEmpty(str);
        }

        if (isNull)
            throw new ArgumentNullException(paramName, message ?? "Object cannot be null or empty.");
        return obj;
    }

    /// <summary>
    /// Ensures that the specified IList is not null or empty. Throws an <see cref="ArgumentNullException"/> if the list is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to validate.</param>
    /// <param name="message">An optional message to include in the exception. If not provided, a default message is used.</param>
    /// <param name="paramName">The name of the parameter being validated. This is automatically populated by the compiler.</param>
    /// <returns>The validated list if it is not null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is null or empty.</exception>
    public static IList<T> IsNullThrow<T>(
    [NotNullWhen(false)] this IList<T>? list,
    string? message = null,
    [CallerArgumentExpression(nameof(list))] string? paramName = null)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentNullException(paramName, message ?? "List cannot be null or empty.");
        return list;
    }

    /// <summary>
    /// Determines whether the specified object is null or, if it is a string, empty.
    /// </summary>
    /// <typeparam name="T">The type of the object to check.</typeparam>
    /// <param name="obj">The object to check for null or empty.</param>
    /// <returns>
    /// <c>true</c> if the object is null or, if it is a string, empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T obj)
    {
        if (obj is string str)
            return string.IsNullOrEmpty(str);

        return obj == null;
    }

    /// <summary>
    /// Determines whether the specified object is not null.
    /// </summary>
    /// <typeparam name="T">The type of the object to check.</typeparam>
    /// <param name="obj">The object to check for not null.</param>
    /// <returns><c>true</c> if the object is not null; otherwise, <c>false</c>.</returns>
    public static bool IsNotNull<T>([NotNullWhen(false)] this T obj)
    {
        if (obj is string str)
            return !string.IsNullOrEmpty(str);

        return obj != null;
    }
}

#nullable enable
/// <summary>
/// Provides extension methods for <see cref="Dictionary{TKey, TValue}"/> to enhance functionality.
/// </summary>
/// <remarks>This class includes methods that simplify common operations on dictionaries, such as removing entries
/// with null values.</remarks>
public static class DictionaryExtensions
{
    /// <summary>
    /// Removes all entries with <see langword="null"/> values from the specified dictionary.
    /// </summary>
    /// <remarks>This method iterates over the dictionary and removes any key-value pairs where the value is 
    /// <see langword="null"/>. The operation modifies the original dictionary and also returns it  for
    /// convenience.</remarks>
    /// <param name="dict">The dictionary from which to remove entries with <see langword="null"/> values.  This dictionary is modified in
    /// place.</param>
    /// <returns>The modified dictionary with all entries containing <see langword="null"/> values removed.</returns>
    public static Dictionary<string, object?> RemoveNullValues(this Dictionary<string, object?> dict)
    {
        var keys = new List<string>(dict.Keys);
        foreach (var key in keys)
        {
            if (dict[key] == null)
                dict.Remove(key);
        }
        return dict;
    }
}
