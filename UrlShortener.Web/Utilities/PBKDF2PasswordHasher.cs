using System.Security.Cryptography;
using UrlShortener.Web.Domain.Interfaces;

namespace UrlShortener.Web.Utilities;

/// <summary>
/// Implements secure password hashing using the PBKDF2 algorithm.
/// This method uses key stretching to defend against brute-force attacks.
/// </summary>  
public class PbkdfPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 16 bytes for salt
    private const int KeySize = 32;  // 32 bytes for key
    private const int Iterations = 10000; // Number of iterations
    
    // Hash algorithm to use with PBKDF2.
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    
    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        byte[] hash = DeriveKey(password, salt, Iterations, Algorithm);
        
        // Combine salt and hash for storage (e.g., "salt:hash")
        return string.Join(":", Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string passwordHash, string password)
    {
        string[] parts = passwordHash.Split(':');
        
        if (parts.Length != 2)
            return false;
        
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);
        
        byte[] newHash = DeriveKey(password, salt, Iterations, Algorithm);
        
        // Use CryptographicOperations for constant-time comparison (prevents timing attacks)
        return CryptographicOperations.FixedTimeEquals(newHash, hash);
    }
    
    /// <summary>
    /// Derives a cryptographic key from the given password and salt
    /// using PBKDF2 with the specified hash algorithm and iteration count.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="salt">The salt to combine with the password.</param>
    /// <param name="iterations">Number of PBKDF2 iterations.</param>
    /// <param name="algorithm">Hash algorithm to use internally.</param>
    /// <returns>A byte array containing the derived key.</returns>
    private static byte[] DeriveKey(string password, byte[] salt, int iterations, HashAlgorithmName algorithm)
    {
        return new Rfc2898DeriveBytes(password, salt, iterations, algorithm).GetBytes(KeySize);
    }
}