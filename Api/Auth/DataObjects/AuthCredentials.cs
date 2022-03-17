namespace Api.Auth.DataObjects;
public class AuthCredentials{
    public string AccountName { get; set; }
    public string? Password { get; set; }
    public string? RefreshToken { get; set; }
}