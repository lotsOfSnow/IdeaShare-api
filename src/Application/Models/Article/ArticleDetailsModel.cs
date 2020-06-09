using IdeaShare.Application.Models.AppUserModels;

namespace IdeaShare.Application.Models.ArticleModels
{
    public sealed class ArticleDetailsModel : ArticleBaseModel
    {
        public string Body { get; set; }

        public string Description { get; set; }

        public AppUserDetailsModel Author { get; set; }
    }
}
