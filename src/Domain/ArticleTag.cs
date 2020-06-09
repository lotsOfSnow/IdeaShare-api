using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaShare.Domain
{
    public sealed class ArticleTag
    {
        [ForeignKey("Tag")]
        public string TagId { get; set; }
        public Tag Tag { get; set; }

        [ForeignKey("Article")]
        public int ArticleId { get; set; }
        public Article Article { get; set; }
    }
}
