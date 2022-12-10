﻿using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTrack.Api.Features.Auth.Entities;
using TimeTrack.Api.Options;

namespace TimeTrack.Api.Controllers
{
    [Authorize]
    public class AuthController : ControllerBase
    {
        private static List<User> UserList = new List<User>();
        private readonly AuthOptions _authOptions;

        public AuthController(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
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

            var user = UserList.Where(x => x.Username == payload.Name).FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Username = payload.Name
                };
                UserList.Add(user);
            }

            SetCookie(user);
            return Ok();
        }

        private void SetCookie(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authOptions.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new Claim("id", user.Username) }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
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
