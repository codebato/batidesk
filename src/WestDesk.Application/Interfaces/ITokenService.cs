using WestDesk.Domain.Entities;

namespace WestDesk.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}