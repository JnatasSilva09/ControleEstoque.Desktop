using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ControleEstoque.Desktop.Helpers;

public static class PasswordHelper
{
    // Usado só para conferir senhas antigas (formato anterior ao BCrypt)
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

        // O BCrypt só considera os primeiros 72 caracteres
        if (password.Length > 72)
            return (false, "A senha deve ter no máximo 72 caracteres.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "A senha deve conter pelo menos uma letra maiúscula.");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "A senha deve conter pelo menos uma letra minúscula.");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return (false, "A senha deve conter pelo menos um número.");

        if (!Regex.IsMatch(password, @"[^\p{L}\p{N}]"))
            return (false, "A senha deve conter pelo menos um caractere especial.");

        return (true, "Senha válida.");
    }

    // Gera o hash com BCrypt (o salt já fica dentro do próprio hash)
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    public static bool VerifyPassword(string password, string hashGuardado)
    {
        if (string.IsNullOrEmpty(hashGuardado))
            return false;

        // Hash novo (BCrypt começa com "$2")
        if (hashGuardado.StartsWith("$2"))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashGuardado);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Hash antigo: ainda aceita para ninguém ficar sem acesso
        return VerificarHashAntigo(password, hashGuardado);
    }

    // Indica se o hash guardado ainda está no formato antigo e deve ser regravado com BCrypt
    public static bool PrecisaAtualizarHash(string hashGuardado)
    {
        return !string.IsNullOrEmpty(hashGuardado) && !hashGuardado.StartsWith("$2");
    }

    private static bool VerificarHashAntigo(string password, string base64Hash)
    {
        try
        {
            byte[] hashBytes = Convert.FromBase64String(base64Hash);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            byte[] esperado = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, esperado, 0, HashSize);

            return CryptographicOperations.FixedTimeEquals(hash, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
