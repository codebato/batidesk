using Microsoft.AspNetCore.Mvc;
using NvnDesk.Application.DTOs.Auth;
using NvnDesk.Application.Interfaces;

namespace NvnDesk.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        
        try
        {
            var result = await _authService.RegisterAsync(request);
            // Ok(result) -> HTTP 200 + result'ı JSON olarak client'a döner.
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
           
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
           
            return Unauthorized(new { message = ex.Message });
        }
    }
}