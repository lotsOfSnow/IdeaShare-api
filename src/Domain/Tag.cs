using System.Collections.Generic;

namespace IdeaShare.Domain
{
    public sealed class Tag
    {
        public string Id { get; set; }

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    }
}
