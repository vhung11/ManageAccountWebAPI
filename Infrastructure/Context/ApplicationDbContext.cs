using ManageAccountWebAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace ManageAccountWebAPI.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("OracleConnection");
                optionsBuilder.UseOracle(connectionString);
            }
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<AccountBalance> AccountBalances { get; set; } = null!;
        public DbSet<InterestType> InterestTypes { get; set; } = null!;
        public DbSet<AppLog> AppLogs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.AccountBalances)
                .WithOne()
                .HasForeignKey(ab => ab.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AccountBalance>()
                .HasOne(ab => ab.InterestType)
                .WithMany()
                .HasForeignKey(ab => ab.InterestTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình precision cho decimal properties (Oracle yêu cầu)
            modelBuilder.Entity<AccountBalance>()
                .Property(ab => ab.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InterestType>()
                .Property(it => it.Rate)
                .HasPrecision(5, 4);

            modelBuilder.Entity<AppLog>(entity =>
            {
                entity.ToTable("APP_LOGS");

                entity.Property(e => e.Id)
                    .HasColumnName("ID");

                entity.Property(e => e.LoggedAt)
                    .HasColumnName("LOGGED_AT")
                    .HasColumnType("TIMESTAMP(6)")
                    .HasDefaultValueSql("SYSTIMESTAMP")
                    .IsRequired();

                entity.Property(e => e.LogLevel)
                    .HasColumnName("LOG_LEVEL")
                    .HasMaxLength(16)
                    .IsRequired();

                entity.Property(e => e.Logger)
                    .HasColumnName("LOGGER")
                    .HasMaxLength(256);

                entity.Property(e => e.Message)
                    .HasColumnName("MESSAGE")
                    .HasColumnType("CLOB");

                entity.Property(e => e.Exception)
                    .HasColumnName("EXCEPTION")
                    .HasColumnType("CLOB");

                entity.Property(e => e.Properties)
                    .HasColumnName("PROPERTIES")
                    .HasColumnType("CLOB");

                entity.Property(e => e.MachineName)
                    .HasColumnName("MACHINE_NAME")
                    .HasMaxLength(128);

                entity.Property(e => e.AppName)
                    .HasColumnName("APP_NAME")
                    .HasMaxLength(64);

                entity.HasIndex(e => e.LoggedAt)
                    .HasDatabaseName("IX_APP_LOGS_LOGGED_AT");

                entity.HasIndex(e => new { e.LogLevel, e.LoggedAt })
                    .HasDatabaseName("IX_APP_LOGS_LEVEL_LOGGED_AT");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Permission)
                    .WithMany()
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions)
                .WithOne()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Code)
                .IsUnique();
        }
    }
}