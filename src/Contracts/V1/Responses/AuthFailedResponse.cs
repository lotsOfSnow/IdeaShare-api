using System.Collections.Generic;

namespace IdeaShare.Contracts.V1.Responses
{
    public class AuthFailedResponse
    {
        public IDictionary<string, string> Errors { get; set; }
    }
}
