using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Domain
{
    public sealed class AppUser : IdentityUser
    {
        [MaxLength(20, ErrorMessage = "First name length cannot be greater than 20")]
        public string FirstName { get; set; }

        [MaxLength(20, ErrorMessage = "Last name length cannot be greater than 20")]
        public string LastName { get; set; }

        public bool IsRealNameHidden { get; set; } = true;

        public DateTime RegistrationDate { get; set; }

        public string ProfilePicture { get; set; }

        public string Description { get; set; }

        public ICollection<Article> Articles { get; set; } = new List<Article>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
