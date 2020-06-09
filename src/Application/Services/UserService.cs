using IdeaShare.Domain;
using IdeaShare.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaShare.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _context.Users.Include(x => x.Articles).ThenInclude(x => x.Likes).FirstOrDefaultAsync(x => x.UserName == id);
        }

        public async Task<string> GetIdFromUsernameAsync(string username)
        {
            return (await _context.Users.Where(x => x.UserName == username).FirstOrDefaultAsync())?.Id;
        }
    }
}
