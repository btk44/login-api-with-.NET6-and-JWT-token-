namespace Api.Auth.DataObjects;

public class TokenData{
    public string Token { get; set; } 
    public string RefreshToken { get; set; }
    public DateTime ExpirationTime { get; set; }
    public DateTime RefreshExpirationTime { get; set; }
}