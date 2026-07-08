using Authentication.Mfa.Twilio.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Mfa.Twilio.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<MfaTransaction> MfaTransactions => Set<MfaTransaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MfaDevice> MfaDevices => Set<MfaDevice>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<MfaTransaction>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Purpose, e.IsUsed });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}