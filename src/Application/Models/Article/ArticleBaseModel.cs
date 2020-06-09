using System;
using System.Collections.Generic;

namespace IdeaShare.Application.Models.ArticleModels
{
    public class ArticleBaseModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }
        
        public IList<string> Tags { get; set; }

        public int Likes { get; set; }

        public string FeaturedImage { get; set; }
    }
}
