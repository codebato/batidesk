using System.IdentityModel.Tokens.Jwt;   // JWT token oluşturmak için .NET'in kendi kütüphanesi
using System.Security.Claims;             // "Claim" = token içine gömdüğümüz bilgi parçası (örn. UserId, TenantId)
using System.Text;
using Microsoft.Extensions.Configuration; // appsettings.json'dan JWT ayarlarını okumak için
using Microsoft.IdentityModel.Tokens;
using NvnDesk.Application.Interfaces;
using NvnDesk.Domain.Entities;

namespace NvnDesk.Infrastructure.Services;

public class TokenService : ITokenService
{
    // 📌 STANDART: appsettings.json'daki ayarları constructor'da IConfiguration üzerinden okumak
    // klasik bir .NET pratiği — her yerde bunu göreceksin, secret/key gibi değerleri koda
    // hardcode etmek yerine dışarıdan (config dosyasından) okuruz.
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Claim: token'ın içine "damgaladığımız" bilgiler. Token'ı çözen taraf
        // (bizim API'miz) bu bilgilere token'ı decode ederek ulaşabilir, veritabanına
        // tekrar sorgu atmasına gerek kalmaz. Bu yüzden her istekte DB'ye gitmeden
        // "bu kullanıcı hangi tenant'a ait, rolü ne" bilgisini hızlıca öğreniriz.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // "Subject" = token kime ait
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenantId", user.TenantId.ToString()),            // ÖZEL CLAIM: bizim projeye özel, multi-tenancy için şart
            new Claim(ClaimTypes.Role, user.Role.ToString())            // rol bazlı yetkilendirme (Admin/Agent/Customer) için
        };

        // JWT'yi imzalamak için kullanılan gizli anahtar. appsettings.json'daki
        // "Jwt:Secret" değerinden okunuyor (bunu birazdan ekleyeceğiz).
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

        // 📌 STANDART: HmacSha256, JWT imzalama için en yaygın kullanılan algoritmalardan biri.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // token 2 saat sonra geçersiz olacak
            signingCredentials: credentials
        );

        // Token nesnesini, gerçek bir string'e (client'a gönderilecek hale) çeviriyoruz.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}