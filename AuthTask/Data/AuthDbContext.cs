using AuthTask.Data.ValueGenerators;
using AuthTask.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthTask.Data
{
    public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(x => x.IsActive).HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email);

            modelBuilder.Entity<User>()
                .Property(x => x.RegistrationTime)
                .HasValueGenerator<UtcDateTimeValueGenerator>()
                .ValueGeneratedOnAdd();
        }
    }
}
