using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Contracts.V1.Requests
{
    public class AppUserRegistrationRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
