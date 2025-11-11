using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;


namespace PersonelLeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(KullaniciRegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result ? Ok("Kayıt Başarılı") : BadRequest("Bu eposta zaten kayıtlı");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(KullaniciLoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized("Geçersiz eposta veya şifre");

        return Ok(new { Token=token });
    }

    [Authorize]
    [HttpGet("Secret")]
    public IActionResult SecretArea() => Ok("Sadece token ile bu mesajı görebilirsiniz.");
}
