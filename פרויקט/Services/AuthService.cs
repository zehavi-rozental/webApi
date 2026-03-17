using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;
using MyMiddleware.Models;
using ServiceUsers.interfaces;

namespace MyMiddleware.Services
{
    public interface IAuthService
    {
        Task<string?> ValidateGoogleTokenAsync(string idToken);
    }

    public class AuthService : IAuthService
    {
        private const string GoogleClientId = "405923075659-0bu4h5l9iobf933t9vqu4ebq6a4543ls.apps.googleusercontent.com";

        private readonly IIUsers _users;
        private readonly string _configuredClientId;

        public AuthService(IIUsers users, IConfiguration configuration)
        {
            _users = users;
            _configuredClientId = configuration["Google:ClientId"] ?? string.Empty;
        }

        public async Task<string?> ValidateGoogleTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                return null;

            // Prefer the configured client ID, but fall back to the hard-coded default.
            var clientId = !string.IsNullOrWhiteSpace(_configuredClientId) ? _configuredClientId : GoogleClientId;

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            }
            catch
            {
                // Invalid token / expired / signature mismatch
                return null;
            }

            // Hard check the Audience against our client ID
            // Audience may be a single string or an array of strings.
            if (payload.Audience is null)
                return null;

            var audienceValid = payload.Audience switch
            {
                string single => string.Equals(single, clientId, StringComparison.Ordinal),
                IEnumerable<string> list => list.Contains(clientId, StringComparer.Ordinal),
                IEnumerable<object> listObj => listObj.Any(o => string.Equals(o?.ToString(), clientId, StringComparison.Ordinal)),
                _ => false
            };

            if (!audienceValid)
                return null;

            var email = payload.Email;
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Use the display name from Google, falling back to given/family name or email if not provided.
            var displayName = payload.Name;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                var given = payload.GivenName?.Trim();
                var family = payload.FamilyName?.Trim();
                displayName = string.IsNullOrWhiteSpace(given) && string.IsNullOrWhiteSpace(family)
                    ? email
                    : string.Join(' ', new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            // Find existing user by email (preferred) or by legacy Name field (email stored there)
            var user = _users.GetAll().FirstOrDefault(u =>
                (!string.IsNullOrWhiteSpace(u.Email) && string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)) ||
                (string.IsNullOrWhiteSpace(u.Email) && string.Equals(u.Name, email, StringComparison.OrdinalIgnoreCase)));

            if (user == null)
            {
                user = new User
                {
                    Name = displayName,
                    Email = email,
                    Password = string.Empty,
                    Role = "User"
                };

                _users.Add(user);
            }
            else
            {
                // Ensure the user record reflects the Google display name and email.
                var updated = false;

                if (!string.Equals(user.Name, displayName, StringComparison.Ordinal))
                {
                    user.Name = displayName;
                    updated = true;
                }

                // Always set email (even if it was empty) to keep the account linked by email.
                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    updated = true;
                }

                if (updated)
                {
                    _users.Update(user);
                }
            }

            // Generate our own JWT token so existing [Authorize] logic remains unchanged
            // Include both the standard ClaimTypes.Name and a plain "name" claim for easier JS access.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("name", user.Name),
                new Claim("unique_name", user.Name),
                new Claim("username", user.Name),
                new Claim("email", user.Email),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = TokenService.GetToken(claims);
            return TokenService.WriteToken(token);
        }
    }
}
