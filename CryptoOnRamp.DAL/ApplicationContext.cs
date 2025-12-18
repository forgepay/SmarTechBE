using CryptoOnRamp.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoOnRamp.DAL;

public class ApplicationContext : DbContext
{

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.Entity<UserDb>(x =>
        {
            x.ToTable("Users");
            x.Property(x => x.PasswordHash).HasColumnName("PasswordHash");
            x.Property(x => x.Email).HasColumnName("Email");
            x.Property(x => x.Role).HasConversion<string>();
            x.Property(x => x.RegistrationStep).HasConversion<string>();
            x.Property(x => x.Language).HasConversion<string>()
                .HasDefaultValue(AppLanguage.English);

            x.Property(u => u.WebhookUrl)
                .HasColumnName("WebhookUrl")
                .IsRequired(false);
            x.Property(u => u.WebhookAuthorizationToken)
                .HasColumnName("WebhookAuthorizationToken")
                .IsRequired(false);
            
            x.Property(u => u.ApiKeyName)
                .HasColumnName("ApiKeyName")
                .IsRequired(false);
            x.Property(u => u.ApiKeyHash)
                .HasColumnName("ApiKeyHash")
                .IsRequired(false);
            x.HasIndex(u => u.ApiKeyHash);

            x.HasIndex(u => u.Name)
                .HasFilter("\"DeletedAt\" is null")
                .IsUnique();

            x.HasIndex(u => u.Email)
                .HasFilter("\"DeletedAt\" is null")
                .IsUnique();
        });

        modelBuilder.Entity<SessionDb>(x =>
        {
            x.ToTable("Sessions");
        });

        modelBuilder.Entity<TransactionDb>(x =>
        {
            x.ToTable("Transactions");
            x.Property(x => x.Status).HasConversion<string>();
            x.HasMany(x => x.CheckoutSessions).WithOne(x => x.Transaction).HasForeignKey(x => x.TransactionId);
            x.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            x.HasMany(x => x.Payouts).WithOne(x => x.Transaction).HasForeignKey(x => x.TransactionId);
        });

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FeeSchemeDb>(x =>
        {
            x.ToTable("FeeSchemes");
            x.Property(x => x.Type).HasConversion<string>();
        });

        modelBuilder.Entity<CheckoutSessionDb>(x =>
        {
            x.ToTable("CheckoutSessions");
            x.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<PayoutDb>(x =>
        {
            x.ToTable("Payouts");
            x.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<TelegramUserDb>(x =>
        {
            x.ToTable("TelegramUsers");
            x.HasKey(t => t.TelegramId);
            x.HasIndex(t => t.UserId).IsUnique(false);
            x.HasOne<UserDb>().WithMany().HasForeignKey(t => t.UserId);

            x.Property(t => t.TelegramId).HasColumnName("TelegramId").ValueGeneratedNever();
            x.Property(t => t.UserId).HasColumnName("UserId");
        });

    }
}
