using IdeaShare.Domain;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdeaShare.Application.Services
{
    public interface IUserService
    {
        Task<AppUser> GetUserAsync(string id);
        Task<string> GetIdFromUsernameAsync(string username);
    }
}
