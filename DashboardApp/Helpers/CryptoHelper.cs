using System;
using System.Security.Cryptography;
using System.Text;

namespace DashboardApp.Helpers;

public static class CryptoHelper
{
    private static readonly byte[]? _entropy = null;

    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] protectedBytes = ProtectedData.Protect(
            plainBytes,
            _entropy,
            DataProtectionScope.CurrentUser); // Только текущий пользователь Windows

        return Convert.ToBase64String(protectedBytes);
    }

    public static string Unprotect(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            byte[] protectedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = ProtectedData.Unprotect(
                protectedBytes,
                _entropy,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty; // Если не удалось расшифровать (сменился пользователь и т.д.)
        }
    }
}