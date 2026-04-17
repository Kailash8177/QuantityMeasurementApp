using Microsoft.EntityFrameworkCore;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementRepositoryLayer.Repositories.EfCore
{
    public class QuantityMeasurementDbContext : DbContext
    {
        public QuantityMeasurementDbContext(DbContextOptions<QuantityMeasurementDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuantityMeasurementRecord> Measurements => Set<QuantityMeasurementRecord>();
        public DbSet<UserEntity>                Users         => Set<UserEntity>();
        public DbSet<RefreshTokenEntity>        RefreshTokens => Set<RefreshTokenEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Measurement indexes
            var m = modelBuilder.Entity<QuantityMeasurementRecord>();
            m.HasIndex(x => x.OperationType).HasDatabaseName("idx_op_type");
            m.HasIndex(x => x.Operand1Category).HasDatabaseName("idx_op1_cat");
            m.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_created");

            // User indexes
            var u = modelBuilder.Entity<UserEntity>();
            u.HasIndex(x => x.Username).IsUnique().HasDatabaseName("idx_username");
            u.HasIndex(x => x.Email).IsUnique().HasDatabaseName("idx_email");

            // RefreshToken — ignore computed property IsActive
            var rt = modelBuilder.Entity<RefreshTokenEntity>();
            rt.Ignore(x => x.IsActive);
            rt.HasIndex(x => x.Token).HasDatabaseName("idx_refresh_token");
            rt.HasIndex(x => x.UserId).HasDatabaseName("idx_refresh_user");

            // Seed admin user (password: admin123)
            modelBuilder.Entity<UserEntity>().HasData(new UserEntity
            {
                Id           = 1,
                Username     = "admin",
                Email        = "admin@quantitymeasurement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FirstName    = "Admin",
                LastName     = "User",
                Role         = "Admin",
                IsActive     = true,
                CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
