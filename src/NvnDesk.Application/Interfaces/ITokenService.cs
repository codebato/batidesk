using NvnDesk.Domain.Entities;

namespace NvnDesk.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}