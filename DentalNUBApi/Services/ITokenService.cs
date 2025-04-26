using DentalNUB.Api.Entities;

namespace DentalNUB.Api.Services;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}
