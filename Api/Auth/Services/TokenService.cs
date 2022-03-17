using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Auth.DataObjects;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth.Services;

public class TokenService : ITokenService{
    private IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenData CreateTokenData(Dictionary<string, string> tokenClaims)
    {
        var issueTime = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Auth:ServerSigningPassword"]));
        var claims = tokenClaims.Select(tc => new Claim(tc.Key, tc.Value));

        var jwt = new JwtSecurityToken(issuer: _configuration["Auth:Issuer"],
                                        audience: _configuration["Auth:Audience"],
                                        claims: claims,
                                        notBefore: issueTime.AddSeconds(int.Parse(_configuration["Auth:NotBeforeInSec"])),
                                        expires: issueTime.AddSeconds(int.Parse(_configuration["Auth:AccessTokenExpirationTimeInSec"])),
                                        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)); 

        return new TokenData(){
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpirationTime = issueTime.AddSeconds(int.Parse(_configuration["Auth:AccessTokenExpirationTimeInSec"])),
            RefreshToken = GenerateRefreshToken(),
            RefreshExpirationTime = issueTime.AddSeconds(int.Parse(_configuration["Auth:RefreshTokenExpirationTimeInSec"]))
        };
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create()){
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public string GetClaimFromToken(ClaimsPrincipal claimsPrincipal, string claimType){
        var claim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == claimType);

        if(claim != null){
            return claim.Value;
        }

        return string.Empty;
    }

    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration)
    {          
        return new TokenValidationParameters
        {
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Auth:ServerSigningPassword"])),
            ValidAudience = configuration["Auth:Audience"],
            ValidateAudience = true,
            ValidIssuer = configuration["Auth:Issuer"],
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    }
}