using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TimeTrack.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Claim>> Get()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) throw new InvalidOperationException();

            return Ok(identity.Claims.Select(
                x => new Models.Claim
                {
                    Type = x.Type,
                    Value = x.Value
                }));
        }
    }
}
