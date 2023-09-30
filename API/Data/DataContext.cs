using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserLike>().HasKey(e => new { e.SourceUserId, e.TargetUserId });

            modelBuilder.Entity<UserLike>().HasOne(e => e.SourceUser).WithMany(e => e.LikedUsers).HasForeignKey(e => e.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>().HasOne(e => e.TargetUser).WithMany(e => e.LikedByUsers).HasForeignKey(e => e.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}