using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaShare.Domain
{
    public class Like
    {
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        public Article Article { get; set; }

        [ForeignKey("AppUser")]
        public string AppUserId { get; set; }
        
        public AppUser AppUser { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
