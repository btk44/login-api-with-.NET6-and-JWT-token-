using Api.Auth.Database;
using Api.Auth.DataObjects;
using Api.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private ITokenService _tokenService;
    private AuthContext _authContext;
    private IPasswordHasher<string> _passwordHasher;
    private const string UserClaimName = "UserName";

    public AuthController(IConfiguration configuration, ITokenService tokenService, AuthContext dbContext)
    {
        _tokenService = tokenService;
        _authContext = dbContext;
        _passwordHasher = new PasswordHasher<string>();
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenData>> Login([FromBody] AuthCredentials credentials)
    {
        if(string.IsNullOrEmpty(credentials.AccountName) || string.IsNullOrEmpty(credentials.Password)){
            throw new AuthException("Missing login data");
        }

        var account = await GetAccountByName(credentials.AccountName);

        if(_passwordHasher.VerifyHashedPassword(account.Name, account.Password, credentials.Password) != PasswordVerificationResult.Success){
            throw new AuthException("Incorrect password");
        }

        var now = DateTime.Now;
        var tokenData = GenerateTokens(account);
        _authContext.RefreshTokens.RemoveRange(_authContext.RefreshTokens.Where(rf => rf.AccountId == account.Id && rf.ExpiresAt < now));
        await _authContext.SaveChangesAsync();

        return Ok(tokenData);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenData>> Refresh([FromBody] AuthCredentials credentials)
    {
        if(string.IsNullOrEmpty(credentials.AccountName) || string.IsNullOrEmpty(credentials.RefreshToken)){
            throw new AuthException("Missing login data");
        }

        var account = await GetAccountByName(credentials.AccountName);

        var now = DateTime.Now;
        if(!account.RefreshTokens.Any(rf => rf.Token == credentials.RefreshToken && rf.ExpiresAt > now)){
            throw new AuthException("Incorrect or expired refresh token");
        }

        var tokenData = GenerateTokens(account);
        account.RefreshTokens.Remove(account.RefreshTokens.FirstOrDefault(rf => rf.Token == credentials.RefreshToken));
        await _authContext.SaveChangesAsync();

        return Ok(tokenData);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] AuthCredentials credentials)
    {
        var userName = _tokenService.GetClaimFromToken(User, UserClaimName);
        var account = await GetAccountByName(userName);

        account.RefreshTokens.Remove(account.RefreshTokens.FirstOrDefault(rf => rf.Token == credentials.RefreshToken));
        await _authContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<AccountEntity> GetAccountByName(string name){
        var account = await _authContext.Accounts.Include(a => a.RefreshTokens)
                                                    .FirstOrDefaultAsync(a => a.Name == name);

        if(account == null){
            throw new AuthException("Account not found");
        }

        return account;
    }

    private TokenData GenerateTokens(AccountEntity account){
        var tokenData = _tokenService.CreateTokenData(new Dictionary<string, string>() { {UserClaimName, account.Name} });

        if(tokenData == null){
            throw new AuthException("Token generation error");
        }

        var refreshToken = new RefreshTokenEntity() { Token = tokenData.RefreshToken, 
                                                        ExpiresAt = tokenData.RefreshExpirationTime,
                                                        AccountId = account.Id,
                                                        Account = account };
        account.RefreshTokens.Add(refreshToken);

        return tokenData;
    }
}
