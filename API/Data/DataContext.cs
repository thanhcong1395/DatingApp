using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>().HasMany(e => e.UserRoles).WithOne(e => e.User).HasForeignKey(e => e.UserId).IsRequired();
            modelBuilder.Entity<AppRole>().HasMany(e => e.UserRoles).WithOne(e => e.Role).HasForeignKey(e => e.RoleId).IsRequired();

            modelBuilder.Entity<UserLike>().HasKey(e => new { e.SourceUserId, e.TargetUserId });

            modelBuilder.Entity<UserLike>().HasOne(e => e.SourceUser).WithMany(e => e.LikedUsers).HasForeignKey(e => e.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>().HasOne(e => e.TargetUser).WithMany(e => e.LikedByUsers).HasForeignKey(e => e.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>().HasOne(e => e.Sender).WithMany(e => e.MessageSent).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(e => e.Recipient).WithMany(e => e.MessageReceived).OnDelete(DeleteBehavior.Restrict);
        }
    }
}