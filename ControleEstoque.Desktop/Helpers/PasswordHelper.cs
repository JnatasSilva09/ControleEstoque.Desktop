using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ControleEstoque.Desktop.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16; 
    private const int HashSize = 20;  
    private const int Iterations = 10000;

    /// <summary>
    /// Verifica se a senha atende aos critérios mínimos de segurança:
    /// Mínimo de 8 caracteres, contendo letra maiúscula, minúscula, número e caractere especial.
    /// </summary>
    public static (bool IsValid, string Message) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "A senha não pode estar vazia.");

        if (password.Length < 8)
            return (false, "A senha deve ter no mínimo 8 caracteres.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "A senha deve conter pelo menos uma letra maiúscula.");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "A senha deve conter pelo menos uma letra minúscula.");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return (false, "A senha deve conter pelo menos um número.");

      if (!Regex.IsMatch(password, @"[!@#$%^&*(),.??"":{}|<>]"))
            return (false, "A senha deve conter pelo menos um caractere especial.");

        return (true, "Senha válida.");
    }

    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string base64Hash)
    {
        byte[] hashBytes = Convert.FromBase64String(base64Hash);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[SaltSize + i] != hash[i])
            {
                return false;
            }
        }

        return true;
    }
}