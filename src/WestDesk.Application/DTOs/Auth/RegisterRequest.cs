namespace WestDesk.Application.DTOs.Auth;

// Login/Register başarılı olunca client'a geri dönecek cevap.
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;   // JWT token, sonraki isteklerde header'da kullanılacak
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;     // Admin/Agent/Customer — string olarak dönüyoruz, enum client tarafında anlamsız
    public Guid TenantId { get; set; }
}