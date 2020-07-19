using IdeaShare.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdeaShare.Application.Interfaces
{
    public interface ICommentService
    {
        Task<bool> AddAsync(int articleId, string authorId, string commentBody);
        Task<BaseRequestResult> RemoveCommentAsync(int commentId, string requestingUserId);
        Task<RequestWithPayloadResult<IEnumerable<Comment>>> GetForArticleAsync(int articleId);
        Task<IEnumerable<Comment>> GetAllForUserAsync(string userId);
    }
}
