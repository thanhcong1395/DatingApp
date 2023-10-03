using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = this.context.Users.OrderBy(e => e.UserName).AsQueryable();
            var likes = this.context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(e => e.SourceUserId == likesParams.UserId);
                users = likes.Select(e => e.TargetUser);
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(e => e.TargetUserId == likesParams.UserId);
                users = likes.Select(e => e.SourceUser);
            }

            var likedUsers = users.Select(e => new LikeDto
            {
                UserName = e.UserName,
                KnownAs = e.KnownAs,
                Age = e.DateOfBirth.CalculateAge(),
                PhotoUrl = e.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = e.City,
                Id = e.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await this.context.Users.Include(e => e.LikedUsers).FirstOrDefaultAsync(e => e.Id == userId);
        }
    }
}