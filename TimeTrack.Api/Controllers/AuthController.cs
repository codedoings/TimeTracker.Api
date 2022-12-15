using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TimeTrack.Api.Features.Auth.Entities;
using TimeTrack.Api.Options;

namespace TimeTrack.Api.Controllers
{
    [Authorize]
    public class AuthController : ControllerBase
    {
        private static List<User> UserList = new List<User>();
        private readonly AuthOptions _authOptions;
        private readonly RSA _rsa;

        public AuthController(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
            _rsa = RSA.Create();
            _rsa.ImportFromPem(_authOptions.PrivateKey);
        }

        [AllowAnonymous]
        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _authOptions.GoogleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
            if (!payload.EmailVerified)
            {
                return BadRequest("User email is not verified");
            }

            var user = UserList.Where(x => x.Email == payload.Email).FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name
                };
                UserList.Add(user);
            }

            SetCookie(user);
            return Ok();
        }

        private void SetCookie(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("email", user.Email),
                    new Claim("name", user.Name)}),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(key: new RsaSecurityKey(_rsa), algorithm: SecurityAlgorithms.RsaSha256),
                Audience = "TimeTrackerWeb",
                Issuer = "TimeTrackerWebApiTokenIssuer"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var signedJws = tokenHandler.WriteToken(token);

            HttpContext.Response.Cookies.Append("token", signedJws,
                new CookieOptions()
                {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }
    }
}
