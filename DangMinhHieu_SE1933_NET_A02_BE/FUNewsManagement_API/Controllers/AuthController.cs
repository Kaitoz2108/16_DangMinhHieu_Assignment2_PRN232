using System.Security.Claims;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _authService.Login(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(response);
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(idClaim) || !short.TryParse(idClaim, out var accountId))
        {
            return Unauthorized();
        }

        var profile = _authService.GetProfile(accountId, roleClaim);
        if (profile == null)
        {
            return NotFound(new { message = "Account profile not found." });
        }

        return Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize]
    public IActionResult UpdateProfile([FromBody] ProfileUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(idClaim) || !short.TryParse(idClaim, out var accountId))
        {
            return Unauthorized();
        }

        var result = _authService.UpdateProfile(accountId, roleClaim, request);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
