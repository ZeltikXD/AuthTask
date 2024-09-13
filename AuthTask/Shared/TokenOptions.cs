using System.Diagnostics.CodeAnalysis;

namespace AuthTask.Shared
{
    public class TokenOptions
    {
        private string _secretKey = string.Empty;
        private string _audience = string.Empty;
        private string _issuer = string.Empty;
        private double _expiresIn;

        public string SecretKey => _secretKey;

        public double ExpiresInHours => _expiresIn;

        public string Audience => _audience;

        public string Issuer => _issuer;

        public TokenOptions SetSecretKey([NotNull]string? secretKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey, nameof(secretKey));
            _secretKey = secretKey;
            return this;
        }

        public TokenOptions SetAudience([NotNull]string? audience)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(audience, nameof(audience));
            _audience = audience;
            return this;
        }

        public TokenOptions SetIssuer([NotNull]string? issuer)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer, nameof(issuer));
            _issuer = issuer;
            return this;
        }

        public TokenOptions SetExpiresInHours(double expiresInHours)
        {
            _expiresIn = expiresInHours;
            return this;
        }
    }
}
