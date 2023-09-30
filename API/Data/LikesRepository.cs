using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await this.context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            var users = this.context.Users.OrderBy(e => e.UserName).AsQueryable();
            var likes = this.context.Likes.AsQueryable();

            if (predicate == "liked")
            {
                likes = likes.Where(e => e.SourceUserId == userId);
                users = likes.Select(e => e.TargetUser);
            }

            if (predicate == "likedBy")
            {
                likes = likes.Where(e => e.TargetUserId == userId);
                users = likes.Select(e => e.SourceUser);
            }

            return await users.Select(e => new LikeDto
            {
                UserName = e.UserName,
                KnownAs = e.KnownAs,
                Age = e.DateOfBirth.CalculateAge(),
                PhotoUrl = e.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = e.City,
                Id = e.Id
            }).ToListAsync();
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await this.context.Users.Include(e => e.LikedUsers).FirstOrDefaultAsync(e => e.Id == userId);
        }
    }
}