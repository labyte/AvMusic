using System.Security.Cryptography;
using System.Text;

namespace AvMusic.Services;

/// <summary>
/// 本地凭据保护（Windows 使用 DPAPI，其他平台 Base64 编码）。
/// </summary>
public static class SecretProtector
{
    public static string? Protect(string? plain)
    {
        if (string.IsNullOrEmpty(plain))
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes(plain);
        if (OperatingSystem.IsWindows())
        {
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        return Convert.ToBase64String(bytes);
    }

    public static string? Unprotect(string? protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(protectedText);
            if (OperatingSystem.IsWindows())
            {
                var plain = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plain);
            }

            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
