using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdeaShare.Application.Interfaces
{
    public interface IArticleService
    {

        Task<int> GetCountAsync(string? tag);

        /// <summary>
        /// Adds a new article to the database, associates it with its author and persists the changes.
        /// </summary>
        /// <param name="article">Article to be added to the database.</param>
        /// <returns></returns>
        Task<RequestWithPayloadResult<Article>> CreateAsync(string authorId, ArticleCreationModel articleModel);

        Task<BaseRequestResult> RemoveAsync(string requestingUserId, int articleId);

        /// <summary>
        /// Retrieves an article from the database based on the provided updated article's Id and modifies its fields, then persists the changes.
        /// </summary>
        /// <param name="updatedArticle">The article to be updated.</param>
        /// <returns></returns>
        Task<RequestWithPayloadResult<Article>> UpdateAsync(ArticleEditModel updatedArticleModel, string userId);

        /// <summary>
        /// Returns all articles from the database.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Article>> GetRangeAsync(int startIndex, int length, string sort, IEnumerable<string> filterTitle, string tag);

        /// <summary>
        /// Returns all articles created by the user with the specified Id.
        /// </summary>
        /// <param name="userId">The Id of the user whose articles are to be returned.</param>
        /// <returns></returns>
        Task<IEnumerable<Article>> GetAllAsync(string userId);

        Task<RequestWithPayloadResult<IEnumerable<Article>>> GetByUserAsync(string username, string sort);

        Task<RequestWithPayloadResult<IEnumerable<Article>>> GetLikedByUserAsync(string username, string sort);

        Task<RequestWithPayloadResult<bool>> IsArticleLikedByUserAsync(string username, int articleId);

        /// <summary>
        /// Returns a single article from the database.
        /// </summary>
        /// <param name="id">The id of the article to return.</param>
        /// <returns></returns>
        Task<Article> GetAsync(int id);

        Task<IEnumerable<Article>> GetByTagAsync(string tag);

        Task<RequestWithPayloadResult<Like>> AddLikeAsync(string userId, int articleId);

        Task<BaseRequestResult> RemoveLikeByUserAsync(string userId, int articleId);
    }
}
