using System;
using System.Security.Cryptography;
using System.Text;

namespace ClipX2.UI
{
    /// <summary>
    /// Keeps secrets (the LanguageTool API key) out of settings.json in plain text: on Windows
    /// they are encrypted with DPAPI for the current Windows user, so the file is useless on
    /// another account or machine. (Elsewhere, should ClipX go cross-platform, the platform's
    /// own store would take this class's place: the macOS Keychain, libsecret on Linux.)
    /// </summary>
    internal static class SecretStore
    {
        // Ties the encrypted value to ClipX, so another program's DPAPI data can't be passed off as it
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ClipX2 settings secret");

        /// <summary>The secret encrypted, as Base64 ("" for none).</summary>
        public static string Protect(string secret)
        {
            if (string.IsNullOrEmpty(secret)) return "";
            byte[] data = ProtectedData.Protect(Encoding.UTF8.GetBytes(secret), Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(data);
        }

        /// <summary>The secret back ("" if there is none, or it was encrypted by another user or machine).</summary>
        public static string Unprotect(string protectedSecret)
        {
            if (string.IsNullOrEmpty(protectedSecret)) return "";
            try
            {
                byte[] data = ProtectedData.Unprotect(Convert.FromBase64String(protectedSecret), Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch (Exception ex) when (ex is CryptographicException || ex is FormatException)
            {
                return ""; // e.g. settings imported from another account: the key must be entered again
            }
        }
    }
}
