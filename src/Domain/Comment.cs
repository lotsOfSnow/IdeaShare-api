using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaShare.Domain
{
    public sealed class Comment
    {
        public int Id { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public AppUser Author { get; set; }

        [MinLength(5, ErrorMessage = "Content length cannot be less than 5")]
        [MaxLength(300, ErrorMessage = "Content length cannot be greater than 300")]
        public string Body { get; set; }

        public DateTime TimeSent { get; set; }

        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        public Article Article { get; set; }
        
        public bool IsAccepted { get; set; }

        public bool IsRejected { get; set; }

        public bool IsPending
        {
            get => !IsAccepted && !IsRejected;
        }
    }
}
