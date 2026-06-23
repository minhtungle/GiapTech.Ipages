using GiapTech.Ipages.Domain.Entities;

namespace GiapTech.Ipages.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
