
namespace Movies.Api.Sdk
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "/api";
        public static class Movies
        {
            private const string Base = $"{ApiBase}/movies";

            public const string Create = Base;
            public const string Get = $"{Base}/{{idOrSlug}}";
            public const string GetAll = Base;
            public const string Update = $"{Base}/{{id}}";
            public const string Delete = $"{Base}/{{id}}";

            public const string Rate = $"{Base}/{{id}}/ratings";
            public const string DeleteRating = $"{Base}/{{id}}/ratings";
        }

        public static class Ratings
        {
            private const string Base = $"{ApiBase}/ratings";

            public const string GetUserRating = $"{Base}/me";
        }

        public static class Genres
        {
            private const string Base = $"{ApiBase}/genres";

            public const string GetAll = Base;
        }

        public static class Identity
        {
            private const string Base = $"{ApiBase}/identity";

            public const string Token = $"{Base}/token";
            public const string TokenDirect = "/token";
        }
    }
}
