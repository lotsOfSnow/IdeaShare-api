using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Application.Models.CommentModels
{
    public class PostCommentModel
    {
        [Required]
        public string Body { get; set; }
    }
}
