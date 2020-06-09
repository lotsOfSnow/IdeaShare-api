using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaShare.Domain
{
    public sealed class Article
    {

        [Key]
        public int Id { get; set; }

        [MinLength(5, ErrorMessage = "Title length cannot be less than 5")]
        [MaxLength(30, ErrorMessage = "Title length cannot be greater than 30")]
        public string Title { get; set; }

        [MaxLength(300, ErrorMessage = "Description length cannot be greater than 300")]
        public string Description { get; set; }

        [MinLength(10, ErrorMessage = "Body length cannot be less than 10")]
        [MaxLength(2000, ErrorMessage = "Body length cannot be greater than 2000")]
        public string Body { get; set; }

        public string FeaturedImage { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public AppUser Author { get; set; }

        public ICollection<ArticleTag> TagList { get; set; } = new List<ArticleTag>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>(); 
    }
}
