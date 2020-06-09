using System.Collections.Generic;

namespace IdeaShare.Domain
{
    public class BaseRequestResult
    {
        public bool Success { get; set; }
        public IDictionary<string, string> Errors { get; set; }
    }
}
