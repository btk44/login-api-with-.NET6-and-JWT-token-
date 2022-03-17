using Microsoft.EntityFrameworkCore;

namespace Api.Auth.Database;
public class AuthContext : DbContext{
    public AuthContext(DbContextOptions<AuthContext> options): base(options){ }
    public DbSet<AccountEntity> Accounts { get; set;} 
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) { }
}