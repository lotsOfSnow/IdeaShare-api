using System.ComponentModel.DataAnnotations;

namespace IdeaShare.Contracts.V1.Requests
{
    public class AppUserLoginRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(1)]
        public string Password { get; set; }
    }
}
