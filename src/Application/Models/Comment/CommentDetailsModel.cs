using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Models.ArticleModels;
using System;

namespace IdeaShare.Application.Models.CommentModels
{
    public class CommentDetailsModel
    {
        public AppUserBaseModel Author { get; set; }

        public int Id { get; set; }

        public string Body { get; set; }

        public DateTime TimeSent { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsRejected { get; set; }

        public bool IsPending { get; set; }

        public ArticleBasicModel Article { get; set; }
    }
}
