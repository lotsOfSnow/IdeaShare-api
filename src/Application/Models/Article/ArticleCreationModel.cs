using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Application.Models.ArticleModels
{
    public class ArticleCreationModel
    {
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }

        [Required]
        public string Body { get; set; }

        // Comma separated
        public string Tags { get; set; }

        public IFormFile FeaturedImage { get; set; }
    }
}
