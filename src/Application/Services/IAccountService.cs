using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Domain;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace IdeaShare.Application.Services
{
    public interface IAccountService
    {
        Task<RequestWithPayloadResult<AppUser>> RegisterAsync(string username, string email, string password);
        Task<AppUserLoginResult> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<AppUser> GetCurrentUserAsync(string id);
        Task<BaseRequestResult> UpdateAsync(AppUser user, AppUserUpdateModel updateModel);
        Task<IdentityResult> ChangePaswordAsync(string id, string currentPassword, string newPassword);
    }
}
