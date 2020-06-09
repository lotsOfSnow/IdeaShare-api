using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Application.Models.AppUserModels
{
    public class AppUserUpdateModel
    {
        [MaxLength(20, ErrorMessage = "First name length cannot be greater than 20")]
        public string FirstName { get; set; }

        [MaxLength(20, ErrorMessage = "Last name length cannot be greater than 20")]
        public string LastName { get; set; }

        public bool IsRealNameHidden { get; set; }

        [MaxLength(300, ErrorMessage = "Description length cannot be greater than 300")]
        public string Description { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public IFormFile ProfilePicture { get; set; }

        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPasswordConfirmation", ErrorMessage = "New password has to be retyped correctly")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPasswordConfirmation { get; set; }
    }
}
