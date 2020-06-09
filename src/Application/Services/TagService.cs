using IdeaShare.Domain;
using IdeaShare.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaShare.Application.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;

        public TagService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ArticleTag>> ProcessAsync(string tagList, Article article)
        {
            var tags = SplitTags(tagList);
            var articleTags = new List<ArticleTag>(); // the list to which the tags will be appended
            var existingArticleTags = _context.ArticleTags.Where(x => x.Article == article); // query existing articleTags to compare them later and remove entries from db if tag was removed

            for(var i = article.TagList.Count - 1; i >= 0; i--) // here we go through the article's tags and remove the ones that aren't included in new tags
            {
                var tag = article.TagList.ElementAt(i);

                if(!tagList.Contains(tag.TagId))
                {
                    article.TagList.Remove(tag);
                    _context.ArticleTags.Remove(tag);
                }
            }

            foreach (var tag in tags)
            {
                Tag tagFromDatabase = await _context.Tags.FindAsync(tag); // check if such tag exists in the database. if not, create it
                if(tagFromDatabase == null) 
                {
                    var newTag = new Tag { Id = tag };
                    await _context.Tags.AddAsync(newTag);

                    await _context.SaveChangesAsync();

                    tagFromDatabase = newTag;
                }

                var articleAlreadyHasTag = existingArticleTags.FirstOrDefault(x => x.TagId == tagFromDatabase.Id);

                if (articleAlreadyHasTag == null) // only add the new tag information to the database if the article doesn't already have an identical one
                {
                    var articleTag = new ArticleTag { Article = article, Tag = tagFromDatabase };
                    articleTags.Add(articleTag);

                    _context.ArticleTags.Add(articleTag);
                }
                else // if the article has such tag already, just add it to the list of tags that will be returned
                {
                    articleTags.Add(articleAlreadyHasTag);
                }
            }

            return articleTags;
        }

       public async Task<Tag> CreateAsync(string tag)
        {
            // Prevent race condition
            var newTag = new Tag { Id = tag };
            await _context.Tags.AddAsync(newTag);
            try
            {
                await _context.SaveChangesAsync();
                return newTag;
            } 
            catch(Exception e)
            {
                if(e.InnerException.Message.Contains("Violation of PRIMARY KEY constraint"))
                {
                    // We have to detach the entity with duplicate key from this request's DbContext, else EF Core would try to persist it again with the next tag
                    _context.Entry(newTag).State = EntityState.Detached;
                    // Since tags cannot be deleted and such id already exists, this will return the correct object.
                    return await _context.Tags.FindAsync(tag);

                }

                /*
                // implementation for Sqlite:
                var sqlException = e.GetSqlException<SqliteException>();

                if (sqlException != null && sqlException.SqliteErrorCode == 19)
                {
                    _context.Entry(newTag).State = EntityState.Detached;

                    return await _context.Tags.FindAsync(tag);
                }
                */
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> CreateRangeAsync(IEnumerable<string> tags)
         {
            var tagsToReturn = new List<Tag>();

            foreach(var tag in tags)
            {
                tagsToReturn.Add(await CreateAsync(tag));
            }

            return tagsToReturn;
        }

        public void RefreshTags(IEnumerable<ArticleTag> oldTags, IEnumerable<ArticleTag> newTags) // should be removed?
        {
            foreach(var tag in oldTags)
            {
                if(!newTags.Contains(tag))
                {
                    _context.ArticleTags.Remove(tag);
                }
            }
        }

        private IEnumerable<string> SplitTags(string tagList) // put it in utilities
        {
            var tags = tagList.Split(" ").ToList();

            return tags;
        }
    }
}
