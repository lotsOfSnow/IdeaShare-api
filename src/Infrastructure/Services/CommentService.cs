using IdeaShare.Application.Interfaces;
using IdeaShare.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaShare.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AddAsync(int articleId, string authorId, string commentBody)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                return false;
            }

            var user = await _context.Users.FindAsync(authorId);
            if (user == null)
            {
                return false;
            }

            var comment = new Comment
            {
                Body = commentBody,
                AuthorId = authorId,
                TimeSent = DateTime.UtcNow,
                ArticleId = articleId,
            };

            article.Comments.Add(comment);
            user.Comments.Add(comment);
            _context.Comments.Add(comment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<RequestWithPayloadResult<IEnumerable<Comment>>> GetForArticleAsync(int articleId)
        {
            var articleExists = await _context.Articles.FirstOrDefaultAsync(x => x.Id == articleId) != null;

            if (!articleExists)
            {
                return new RequestWithPayloadResult<IEnumerable<Comment>>
                {
                    Errors = new Dictionary<string, string> { { "Error", "Article with the specified Id doesn't exist." } }
                };
            }

            return new RequestWithPayloadResult<IEnumerable<Comment>>
            {
                Success = true,
                Payload = await _context.Comments.Include(x => x.Author).Include(x => x.Article.Author).Where(x => x.ArticleId == articleId).ToListAsync()
            };
        }

        private IQueryable<Comment> QueryAllForUser(string userId)
        {
            var userArticles = _context.Articles.Where(x => x.AuthorId == userId);
            var allComments = _context.Comments.Include(x => x.Author);

            return from comment in allComments
                   join userArticle in userArticles on comment.ArticleId equals userArticle.Id
                   select comment;
        }
        public async Task<IEnumerable<Comment>> GetAllForUserAsync(string userId)
        {
            var q = QueryAllForUser(userId).ToList();

            return await QueryAllForUser(userId).Include(x => x.Author).Include(x => x.Article.Author).ToListAsync();
        }

        public async Task<BaseRequestResult> RemoveCommentAsync(int commentId, string requestingUserId)
        {
            var comment = await _context.Comments.Include(x => x.Article).FirstOrDefaultAsync(x => x.Id == commentId);

            if (comment == null)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "Comment", "Comment with this Id does not exist." } }
                };
            }

            // Only allow the comment to be deleted by its author and the person that wrote the article it was posted below.
            if (comment.AuthorId != requestingUserId && comment.Article.AuthorId != requestingUserId)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "User", "You don't have a permission to do this." } }
                };
            }

            _context.Comments.Remove(comment);

            return new BaseRequestResult
            {
                Success = await _context.SaveChangesAsync() > 0
            };
        }
    }
}
