using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Auth.Database;

[Table("Account")]
public class AccountEntity {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    // Navigation properties
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; }
}