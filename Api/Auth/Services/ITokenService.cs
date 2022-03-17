using System.Security.Claims;
using Api.Auth.DataObjects;

namespace Api.Auth.Services;

public interface ITokenService
{
    TokenData CreateTokenData(Dictionary<string, string> tokenClaims);
    string GenerateRefreshToken();
    string GetClaimFromToken(ClaimsPrincipal claimPrincipal, string claimName);
}