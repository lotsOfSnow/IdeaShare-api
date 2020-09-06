namespace IdeaShare.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version;

        public static class Session
        {
            public const string Get = Base + "/session"; // GET
            public const string Create = Base + "/session"; // POST
            public const string Refresh = Base + "session"; // PUT
            public const string Destroy = Base + "/session"; // DELETE
        }

        public static class Users
        {
            public const string Register = Base + "/users"; // POST
            public const string Get = Base + "/users/{username}"; // GET
            public const string Update = Base + "/users/{username}"; // PATCH

            public static class Articles
            {
                public const string Get = Base + "/users/{username}/articles"; // GET
            }

            public static class Liked
            {
                public const string Get = Base + "/users/{username}/liked"; // GET
            }
        }

        public static class User
        {
            public const string Get = Base + "/user"; // GET

            public static class Liked
            {
                public const string GetSpecific = Base + "/user/liked/{articleId}"; //GET
                public const string GetAll = Base + "/user/liked"; //GET
            }

            public static class Articles
            {
                public const string Comments = Base + "/user/articles/comments"; //GET
            }
        }

        public static class Articles
        {
            public const string GetAll = Base + "/articles"; // GET
            public const string GetSingle = Base + "/articles/{articleId:int}"; // GET
            public const string Create = Base + "/articles"; // GET
            public const string Update = Base + "/articles/{articleId}"; // PATCH
            public const string Delete = Base + "/articles/{articleId}"; // DELETE


            public static class Comments
            {
                public const string Get = Base + "/articles/{articleId}/comments"; // GET
                public const string Create = Base + "/articles/{articleId}/comments"; // POST
                public const string Update = Base + "/articles/{articleId}/comments/{commentId}"; // PATCH
                public const string Delete = Base + "/articles/{articleId}/comments/{commentId}"; // DELETE

            }

            public static class Likes
            {
                public const string Get = Base + "/articles/{articleId}/likes"; // GET
                public const string Create = Base + "/articles/{articleId}/likes"; // POST
                public const string Delete = Base + "/articles/{articleId}/likes"; // DELETE
            }
        }
    }
}
