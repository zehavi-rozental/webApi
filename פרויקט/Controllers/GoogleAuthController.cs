using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMiddleware.Models;
using MyMiddleware.Services;

namespace MyMiddleware.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public GoogleAuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Validates a Google ID token and returns our application's JWT.
        /// </summary>
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] GoogleAuthRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
                return BadRequest();

            var jwt = await _authService.ValidateGoogleTokenAsync(request.IdToken);
            if (string.IsNullOrEmpty(jwt))
                return Unauthorized();

            return Ok(jwt);
        }
    }
}
