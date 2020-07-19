using IdeaShare.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdeaShare.Application.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<ArticleTag>> ProcessAsync(string tagList, Article article);
        void RefreshTags(IEnumerable<ArticleTag> oldTags, IEnumerable<ArticleTag> newTags);

        Task<IEnumerable<Tag>> CreateRangeAsync(IEnumerable<string> tags);
        Task<Tag> CreateAsync(string tag);

    }
}
