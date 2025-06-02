
namespace Movies.Api
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "api";

        public static class V1 
        {
            private const string ApiVersionBase = $"{ApiBase}/v1";
            public static class Movies
            {
                private const string Base = $"{ApiVersionBase}/movies";

                public const string Create = Base;
                public const string Get = $"{Base}/{{idOrSlug}}";
                public const string GetAll = Base;
                public const string Update = $"{Base}/{{id:guid}}";
                public const string Delete = $"{Base}/{{id:guid}}";

                public const string Rate = $"{Base}/{{id:guid}}/ratings";
                public const string DeleteRating = $"{Base}/{{id:guid}}/ratings";
            }

            public static class Ratings
            {
                private const string Base = $"{ApiVersionBase}/ratings";

                public const string GetUserRating = $"{Base}/me";
            }
        }
        
    }
}
