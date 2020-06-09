using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Models.ArticleModels;
using System.Collections.Generic;

namespace IdeaShare.Contracts.V1.Responses
{
    public class ActiveUserResponse
    {
        public AppUserDataModel User { get; set; }
        public IEnumerable<ArticlePreviewModel> LikedArticles { get; set; }
    }
}
