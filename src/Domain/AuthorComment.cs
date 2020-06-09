using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaShare.Domain
{
    public class AuthorComment
    {
        [ForeignKey("Author")]
        public string AuthorId { get; set; }
        public AppUser Author { get; set; }

        [ForeignKey("Comment")]
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
