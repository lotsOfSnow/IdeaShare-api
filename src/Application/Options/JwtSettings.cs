using System;

namespace IdeaShare.Application.Options
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }

        public TimeSpan TokenLifetime { get; set; }
    }
}
