using IdeaShare.Application.Interfaces;
using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Application.Options;
using IdeaShare.Application.Utilities;
using IdeaShare.Domain;
using IdeaShare.Extensions.System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaShare.Infrastructure.Services
{
    public sealed class ArticleService : IArticleService
    {
        private readonly AppDbContext _context;
        private readonly ITagService _tagService;
        private readonly ImageSettings _imageSettings;

        public ArticleService(AppDbContext context, ITagService tagService, ImageSettings imageSettings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _imageSettings = imageSettings ?? throw new ArgumentNullException(nameof(imageSettings));
        }

        public async Task<RequestWithPayloadResult<Article>> CreateAsync(string authorId, ArticleCreationModel articleModel)
        {
            var article = new Article
            {
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = null,
                Title = articleModel.Title,
                Body = articleModel.Body,
                Description = articleModel.Description,
                AuthorId = authorId
            };

            if (articleModel.Tags != null)
            {
                var tagsFromDatabase = await _tagService.CreateRangeAsync(articleModel.Tags.Split(","));
                AttachArticleTags(article, tagsFromDatabase);
            }

            if (articleModel.FeaturedImage != null)
            {
                article.FeaturedImage = await ImageUtilities.ProcessUploadedFile(articleModel.FeaturedImage, _imageSettings.FeaturedImage.Location,
                    _imageSettings.FeaturedImage.Format, _imageSettings.FeaturedImage.Width, _imageSettings.FeaturedImage.Height);
            }

            await _context.Articles.AddAsync(article);
            var savedSuccesfully = await _context.SaveChangesAsync() > 0;

            article = await GetAsync(article.Id);

            if (savedSuccesfully)
            {
                return new RequestWithPayloadResult<Article>
                {
                    Success = savedSuccesfully,
                    Payload = article
                };
            }

            return new RequestWithPayloadResult<Article>();
        }

        public async Task<RequestWithPayloadResult<Article>> UpdateAsync(ArticleEditModel updatedArticleModel, string userId)
        {
            var article = await _context.Articles.Include(x => x.TagList).FirstOrDefaultAsync(x => x.Id == updatedArticleModel.Id);

            if (article == null)
            {
                return new RequestWithPayloadResult<Article>
                {
                    Errors = new Dictionary<string, string> { { "Id", "Such article does not exist" } }
                };
            }

            if (article.AuthorId != userId)
            {
                return new RequestWithPayloadResult<Article>
                {
                    Errors = new Dictionary<string, string> { { "UserId", "This article is not owned by user with the specified Id." } }
                };
            }

            article.Body = updatedArticleModel.Body;
            article.Description = updatedArticleModel.Description;
            article.Title = updatedArticleModel.Title;

            if (updatedArticleModel.FeaturedImage != null)
            {
                article.FeaturedImage = await ImageUtilities.ProcessUploadedFile(updatedArticleModel.FeaturedImage, _imageSettings.FeaturedImage.Location,
                    _imageSettings.FeaturedImage.Format, _imageSettings.FeaturedImage.Width, _imageSettings.FeaturedImage.Height);
            }

            article.LastUpdatedDate = DateTime.UtcNow;

            var tagsFromDatabase = await _tagService.CreateRangeAsync(updatedArticleModel.Tags.Split(","));

            UpdateArticleTags(article, tagsFromDatabase);

            var savedSuccessfully = await _context.SaveChangesAsync() > 0;

            if (savedSuccessfully)
            {
                return new RequestWithPayloadResult<Article>
                {
                    Success = true,
                    Payload = article
                };
            }

            return new RequestWithPayloadResult<Article>()
            {
                Errors = { { "Database", "Couldn't save changes." } }
            };
        }

        public async Task<int> GetCountAsync(string? tag)
        {
            if (tag == null)
            {
                return await _context.Articles.CountAsync();
            }

            return await _context.Articles.Include(x => x.TagList).CountAsync(x => x.TagList.Any(y => y.TagId == tag));
        }

        public async Task<IEnumerable<Article>> GetRangeAsync(int startIndex, int length, string sort, IEnumerable<string> filterTitles, string tag)
        {
            var query = _context.Articles.Include(x => x.Author).Include(x => x.TagList).Include(x => x.Likes).AsQueryable();

            if (filterTitles != null)
            {
                foreach (var title in filterTitles)
                {
                    query = query.Where(x => EF.Functions.Like(x.Title, $"{title}%"));
                }
            }

            if (tag != null)
            {
                query = query.Where(x => x.TagList.Any(y => y.TagId == tag));
            }

            query = Sort(query, sort);
            return (startIndex != -1 && length != -1) ? await query.Skip(startIndex).Take(length).ToListAsync() : await query.ToListAsync();
        }

        public async Task<RequestWithPayloadResult<IEnumerable<Article>>> GetLikedByUserAsync(string username, string sort)
        {
            var query = _context.Articles.Include(x => x.Author).Include(x => x.TagList).Include(x => x.Likes).AsQueryable();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null)
            {
                return new RequestWithPayloadResult<IEnumerable<Article>>
                {
                    Errors = new Dictionary<string, string> { { "User", "User does not exist." } }
                };
            }

            query = query.Where(x => x.Likes.Any(x => x.AppUserId == user.Id));

            query = Sort(query, sort);

            return new RequestWithPayloadResult<IEnumerable<Article>>
            {
                Success = true,
                Payload = await query.ToListAsync()
            };
        }

        private IQueryable<Article> Sort(IQueryable<Article> articlesQueryable, string sort)
        {
            switch (sort)
            {
                case "title":
                    return articlesQueryable.OrderBy(x => x.Title.ToLower());
                case "-title":
                    return articlesQueryable.OrderByDescending(x => x.Title.ToLower());
                case "date":
                    return articlesQueryable.OrderBy(x => x.CreationDate);
                case "-date":
                    return articlesQueryable.OrderByDescending(x => x.CreationDate);
            }

            return articlesQueryable;
        }

        public async Task<IEnumerable<Article>> GetAllAsync(string userId)
        {
            return await _context.Articles.Include(x => x.Author).Include(x => x.TagList).Where(x => x.AuthorId == userId).ToListAsync();
        }

        public async Task<Article> GetAsync(int id)
        {
            return await _context.Articles.Include(x => x.Author).Include(x => x.TagList).Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == id);
        }

        private void AttachArticleTags(Article article, IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                if (article.TagList.FirstOrDefault(x => x.TagId == tag.Id) != null) // if article already has such tag attached to it
                {
                    continue;
                }

                var articleTag = new ArticleTag { Article = article, Tag = tag };
                _context.ArticleTags.Add(articleTag);
            }
        }

        private void UpdateArticleTags(Article article, IEnumerable<Tag> tags)
        {
            for (var i = article.TagList.Count - 1; i >= 0; i--) // We go through the article's tags and remove the ones that aren't included in new tags.
            {
                var articleTag = article.TagList.ElementAt(i);

                if (!tags.Any(x => x.Id == articleTag.TagId))
                {
                    article.TagList.Remove(articleTag);
                }
            }

            AttachArticleTags(article, tags);
        }

        public async Task<IEnumerable<Article>> GetByTagAsync(string tag)
        {
            var articles = _context.Articles.Include(x => x.Author).Include(x => x.TagList);
            var articleTags = _context.ArticleTags.Where(x => x.TagId == tag);

            var query = from article in articles
                        join articleTag in articleTags on article.Id equals articleTag.ArticleId
                        select article;

            return await query.ToListAsync();
        }

        public async Task<RequestWithPayloadResult<IEnumerable<Article>>> GetByUserAsync(string username, string sort)
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username) != null;

            if (!userExists)
            {
                return new RequestWithPayloadResult<IEnumerable<Article>>
                {
                    Errors = new Dictionary<string, string> { { "User", "Such user does not exist." } }
                };
            }

            var query = _context.Articles.Include(x => x.Author).Where(x => x.Author.UserName == username).Include(x => x.TagList).Include(x => x.Likes).AsQueryable();

            query = Sort(query, sort);

            return new RequestWithPayloadResult<IEnumerable<Article>>
            {
                Success = true,
                Payload = await query.ToListAsync()
            };
        }

        public async Task<BaseRequestResult> RemoveAsync(string requestingUserId, int articleId)
        {
            var article = await _context.Articles.FindAsync(articleId);

            if (article == null)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "Article", "Such article does not exist." } }
                };
            }

            if (article.AuthorId != requestingUserId)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "Article", "You don't have a permission to do this." } }
                };
            }

            _context.Articles.Remove(article);

            return new BaseRequestResult
            {
                Success = await _context.SaveChangesAsync() > 0
            };
        }

        public async Task<RequestWithPayloadResult<Like>> AddLikeAsync(string userId, int articleId)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == articleId);

            if (article == null)
            {
                return new RequestWithPayloadResult<Like>
                {
                    Errors = new Dictionary<string, string> { { "Article", "Such article does not exist." } }
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return new RequestWithPayloadResult<Like>
                {
                    Errors = new Dictionary<string, string> { { "User", "Such user does not exist." } }
                };
            }

            var like = new Like
            {
                Article = article,
                ArticleId = article.Id,
                AppUser = user,
                AppUserId = user.Id,
                CreationDate = DateTime.UtcNow,
            };

            await _context.AddAsync(like);

            try
            {
                await _context.SaveChangesAsync();

                return new RequestWithPayloadResult<Like>
                {
                    Success = true,
                    Payload = like
                };
            }
            catch (Exception e)
            {
                var sqlException = e.GetSqlException<SqliteException>();

                if (sqlException != null && sqlException.SqliteErrorCode == 19)
                {
                    _context.Entry(like).State = EntityState.Detached;

                    return new RequestWithPayloadResult<Like>
                    {
                        Errors = new Dictionary<string, string> { { "Like", "Like already exists." } }
                    };
                }
            }

            return new RequestWithPayloadResult<Like>
            {
                Errors = new Dictionary<string, string> { { "Database", "Couldn't persist changes." } }
            };
        }

        public async Task<BaseRequestResult> RemoveLikeByUserAsync(string userId, int articleId)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == articleId);

            if (article == null)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "Article", "Such article does not exist." } }
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "User", "Such user does not exist." } }
                };
            }

            var like = _context.Likes.FirstOrDefault(x => x.AppUserId == userId && x.ArticleId == articleId);

            if (like == null)
            {
                return new BaseRequestResult
                {
                    Errors = new Dictionary<string, string> { { "Like", "Like by this user does not exist." } }
                };
            }

            _context.Likes.Remove(like);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return new BaseRequestResult
                {
                    Success = true,
                };
            }

            return new RequestWithPayloadResult<Like>
            {
                Errors = new Dictionary<string, string> { { "Database", "Couldn't persist changes." } }
            };
        }

        public async Task<RequestWithPayloadResult<bool>> IsArticleLikedByUserAsync(string username, int articleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null)
            {
                return new RequestWithPayloadResult<bool>
                {
                    Errors = new Dictionary<string, string> { { "User", "Such user does not exist." } }
                };
            }

            var article = await _context.Articles.Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == articleId);

            if (article == null)
            {
                return new RequestWithPayloadResult<bool>
                {
                    Errors = new Dictionary<string, string> { { "Article", "Such article does not exist." } }
                };
            }

            return new RequestWithPayloadResult<bool>
            {
                Success = true,
                Payload = article.Likes.Any(x => x.AppUserId == user.Id)
            };
        }
    }
}
