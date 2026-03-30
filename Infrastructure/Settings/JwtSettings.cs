namespace ManageAccountWebAPI.Infrastructure.Settings
{
    /// <summary>
    /// Strongly-typed representation of the "JwtSettings" section in appsettings.json.
    /// Bound via the Options pattern – never read IConfiguration directly.
    /// </summary>
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        /// <summary>HMAC-SHA256 signing secret (≥ 32 characters recommended).</summary>
        public string SecretKey { get; init; } = string.Empty;

        /// <summary>Token issuer (iss claim).</summary>
        public string Issuer { get; init; } = string.Empty;

        /// <summary>Token audience (aud claim).</summary>
        public string Audience { get; init; } = string.Empty;

        /// <summary>Token lifetime in minutes. Defaults to 180.</summary>
        public int ExpirationMinutes { get; init; } = 180;
    }
}
