using System.Security.Cryptography;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Web.Services.UrlShortening;

/// <summary>
/// Generates short codes using Base62 encoding (0-9, a-z, A-Z).
/// Suitable for compact URL identifiers.
/// </summary>
public class Base62ShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string Generate()
    {
        Span<byte> randomBytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(randomBytes);

        ulong value = BitConverter.ToUInt64(randomBytes);

        return EncodeBase62(value);
    }

    /// <summary>
    /// Encodes a numeric value into a Base62 string.
    /// </summary>
    /// <param name="value">
    /// The unsigned integer to convert.
    /// </param>
    /// <returns>
    /// A Base62 representation of the input number.
    /// </returns>
    private static string EncodeBase62(ulong value)
    {
        if (value == 0) return "0";

        Span<char> buffer = stackalloc char[11]; // fits ulong in Base62
        int index = buffer.Length;

        // Repeatedly divide by 62 and record the remainder.
        // Each remainder corresponds to one Base62 digit.
        while (value > 0)
        {
            // Compute the remainder for the current digit.
            ulong remainder = value % (ulong)Alphabet.Length;
            
            // Reduce the value for the next iteration.
            value /= (ulong)Alphabet.Length;
            
            // Write the Base62 character into the buffer.
            buffer[--index] = Alphabet[(int)remainder];
        }

        // Return only the used portion of the buffer.
        return new string(buffer[index..]);
    }
}