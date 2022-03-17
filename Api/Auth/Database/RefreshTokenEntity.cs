using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Auth.Database;

[Table("RefreshToken")]
public class RefreshTokenEntity {
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Navigation properties
    public int AccountId { get; set; }
    public AccountEntity Account { get; set; }
}