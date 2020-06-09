using System;

namespace IdeaShare.Application.Models.AppUserModels
{
    public class AppUserDetailsModel : AppUserBaseModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsRealNameHidden { get; set; }

        public string Description { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int ArticlesWritten { get; set; }

        public int LikesReceived { get; set; }

    }
}
