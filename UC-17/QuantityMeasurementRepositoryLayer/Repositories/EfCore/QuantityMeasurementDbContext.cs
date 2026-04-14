using Microsoft.EntityFrameworkCore;

namespace QuantityMeasurementRepositoryLayer.Repositories.EfCore
{
    public class QuantityMeasurementDbContext : DbContext
    {
        public QuantityMeasurementDbContext(DbContextOptions<QuantityMeasurementDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuantityMeasurementRecord> Measurements => Set<QuantityMeasurementRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<QuantityMeasurementRecord>();
            e.HasIndex(x => x.OperationType).HasDatabaseName("idx_op_type");
            e.HasIndex(x => x.Operand1Category).HasDatabaseName("idx_op1_cat");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_created");
        }
    }
}

