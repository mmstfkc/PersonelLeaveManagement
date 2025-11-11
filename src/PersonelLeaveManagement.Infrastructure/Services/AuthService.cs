using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<bool> RegisterAsync(KullaniciRegisterDto dto)
    {
        if (await _context.Kullanicilar.AnyAsync(x => x.Email == dto.Email))
            return false;

        var kullanici = new Kullanici
        {
            KullaniciAdi = dto.KullaniciAdi,
            Email = dto.Email,
            SifreHash = BCrypt.Net.BCrypt.HashPassword(dto.Sifre),
            Rol = dto.Rol
        };

        _context.Kullanicilar.Add(kullanici);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> LoginAsync(KullaniciLoginDto dto)
    {
        var user = await _context.Kullanicilar.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Sifre, user.SifreHash))
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
