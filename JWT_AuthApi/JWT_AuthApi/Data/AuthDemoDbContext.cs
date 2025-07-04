using JWT_AuthApi.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JWT_AuthApi.Services
{
    public class AuthDemoDbContext : DbContext
    {
        public AuthDemoDbContext(DbContextOptions<AuthDemoDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
        }
    }
}