using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// Provides AES-256-CBC encryption with per-file salt+IV and HMAC-SHA256 authentication.
/// File layout (binary): [salt (16)] [iv (16)] [ciphertext (N)] [hmac (32)]
/// Public API matches the helpers previously used in SaveManager:
/// - EncryptStringToBase64(plain, password)
/// - DecryptStringFromBase64(base64, password)
/// </summary>
public static class AesEncryption
{
    private const int SaltSize = 16;
    private const int IvSize = 16; // AES block size
    private const int KeySize = 32; // 256 bits
    private const int HmacSize = 32; // SHA256 output
    private const int Pbkdf2Iterations = 100_000;

    /// <summary>
    /// Encrypts plainText using password. Returns Base64 string of (salt+iv+ciphertext+hmac).
    /// </summary>
    public static string EncryptStringToBase64(string plainText, string password)
    {
        if (plainText == null) throw new ArgumentNullException(nameof(plainText));
        if (password == null) throw new ArgumentNullException(nameof(password));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // derive key material (AES key + HMAC key)
        var keyMaterialLength = KeySize + HmacSize;
        byte[] keyMaterial;
        using (var kdf = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
        {
            keyMaterial = kdf.GetBytes(keyMaterialLength);
        }

        var aesKey = new byte[KeySize];
        var hmacKey = new byte[HmacSize];
        Array.Copy(keyMaterial, 0, aesKey, 0, KeySize);
        Array.Copy(keyMaterial, KeySize, hmacKey, 0, HmacSize);

        byte[] iv;
        byte[] cipherBytes;
        using (var aes = Aes.Create())
        {
            aes.KeySize = KeySize * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = aesKey;
            aes.GenerateIV();
            iv = aes.IV;

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
                cipherBytes = ms.ToArray();
            }
        }

        // assemble header (salt + iv + cipher)
        var headerLength = SaltSize + IvSize + cipherBytes.Length;
        var header = new byte[headerLength];
        Array.Copy(salt, 0, header, 0, SaltSize);
        Array.Copy(iv, 0, header, SaltSize, IvSize);
        Array.Copy(cipherBytes, 0, header, SaltSize + IvSize, cipherBytes.Length);

        // compute HMAC over header
        byte[] hmac;
        using (var h = new HMACSHA256(hmacKey))
        {
            hmac = h.ComputeHash(header);
        }

        // final = header + hmac
        var finalBytes = new byte[header.Length + hmac.Length];
        Array.Copy(header, 0, finalBytes, 0, header.Length);
        Array.Copy(hmac, 0, finalBytes, header.Length, hmac.Length);

        return Convert.ToBase64String(finalBytes);
    }

    /// <summary>
    /// Decrypts data produced by EncryptStringToBase64 using the same password.
    /// Throws on authentication failure or malformed data.
    /// </summary>
    public static string DecryptStringFromBase64(string base64Input, string password)
    {
        if (base64Input == null) throw new ArgumentNullException(nameof(base64Input));
        if (password == null) throw new ArgumentNullException(nameof(password));

        byte[] allBytes;
        try
        {
            allBytes = Convert.FromBase64String(base64Input);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Input is not valid Base64.", ex);
        }

        if (allBytes.Length < SaltSize + IvSize + HmacSize + 1)
            throw new InvalidDataException("Encrypted data is too short.");

        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        Array.Copy(allBytes, 0, salt, 0, SaltSize);
        Array.Copy(allBytes, SaltSize, iv, 0, IvSize);

        var cipherLength = allBytes.Length - SaltSize - IvSize - HmacSize;
        var cipherBytes = new byte[cipherLength];
        Array.Copy(allBytes, SaltSize + IvSize, cipherBytes, 0, cipherLength);

        var hmacReceived = new byte[HmacSize];
        Array.Copy(allBytes, SaltSize + IvSize + cipherLength, hmacReceived, 0, HmacSize);

        // derive keys
        var keyMaterialLength = KeySize + HmacSize;
        byte[] keyMaterial;
        using (var kdf = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
        {
            keyMaterial = kdf.GetBytes(keyMaterialLength);
        }

        var aesKey = new byte[KeySize];
        var hmacKey = new byte[HmacSize];
        Array.Copy(keyMaterial, 0, aesKey, 0, KeySize);
        Array.Copy(keyMaterial, KeySize, hmacKey, 0, HmacSize);

        // recompute HMAC over (salt + iv + cipher)
        var headerLength = SaltSize + IvSize + cipherBytes.Length;
        var header = new byte[headerLength];
        Array.Copy(allBytes, 0, header, 0, headerLength);

        byte[] hmacComputed;
        using (var h = new HMACSHA256(hmacKey))
        {
            hmacComputed = h.ComputeHash(header);
        }

        if (!FixedTimeEquals(hmacComputed, hmacReceived))
            throw new CryptographicException("HMAC validation failed. Data may be corrupted or password is incorrect.");

        // decrypt
        byte[] plainBytes;
        using (var aes = Aes.Create())
        {
            aes.KeySize = KeySize * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = aesKey;
            aes.IV = iv;

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.FlushFinalBlock();
                plainBytes = ms.ToArray();
            }
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    // Constant-time byte[] comparison to mitigate timing attacks.
    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];

        return diff == 0;
    }
}