using AuthTask.Models;
using AuthTask.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthTask.Interfaces.Implementations
{
    public class AuthManager(IHttpContextAccessor contextAccessor, IUserRepository userRepository,
        IOptions<TokenOptions> options) : IAuthManager
    {
        private readonly HttpContext _context = contextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor), "This class should be instantiated when an HTTP request occurs.");
        private readonly IUserRepository _userRepository = userRepository;
        private readonly TokenOptions _tokenOptions = options.Value;

        Result IAuthManager.SignIn(string email, string password)
        {
            var result = CheckUserExistsAndPassword(email, password);
            if (result.IsFailure) return result;
            var result2 = CheckIsUserBlockedAndSetLastLogin(result);
            if (result2.IsFailure) return result2;
            var token = CreateToken(result, out var expTime);
            _context.Response.Cookies.Append("session_token", token, GetDefaultOptions(expTime));

            return Result.Success();
        }

        Result CheckIsUserBlockedAndSetLastLogin(User user)
        {
            if (!user.IsActive) return Result.Failure("The user cannot login because it's blocked.", StatusCodes.Status403Forbidden);
            var lastLoginRes = _userRepository.SetLastLogin(user.Id);
            if (lastLoginRes.IsFailure) return lastLoginRes;
            return Result.Success();
        }

        Result<User> CheckUserExistsAndPassword(string email, string password)
        {
            var userRes = _userRepository.GetUserByEmail(email);
            if (userRes.IsFailure) return Result.Failure<User>(userRes.Message, userRes.StatusCode);
            if (userRes.Value is null) return Result.Failure<User>("The user doesn't exist.", StatusCodes.Status404NotFound);
            var result = _userRepository.CheckPassword(email, password);
            if (result.IsFailure) return Result.Failure<User>(result.Message, result.StatusCode);
            if (!result) return Result.Failure<User>("Incorrect password.", StatusCodes.Status403Forbidden);
            return userRes!;
        }

        void IAuthManager.SignOut()
        {
            _context.Response.Cookies.Delete("session_token");
            _context.SignOutAsync().GetAwaiter();
        }

        bool IAuthManager.IsSignedIn()
        {
            return _context.Request.Cookies.TryGetValue("session_token", out _);
        }

        private static CookieOptions GetDefaultOptions(DateTimeOffset expiresIn)
            => new()
            {
                SameSite = SameSiteMode.Strict,
                Secure = true,
                HttpOnly = true,
                Expires = expiresIn,
                IsEssential = true
            };

        string CreateToken(User user, out DateTimeOffset expTime)
        {
            var key = Encoding.Unicode.GetBytes(_tokenOptions.SecretKey);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.GivenName, user.Name));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(_tokenOptions.ExpiresInHours),
                Audience = _tokenOptions.Audience,
                Issuer = _tokenOptions.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);
            expTime = tokenDescriptor.Expires.Value;
            return tokenHandler.WriteToken(createdToken);
        }
    }
}
