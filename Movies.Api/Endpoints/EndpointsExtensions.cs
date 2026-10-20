using Movies.Api.Endpoints.Genres;
using Movies.Api.Endpoints.Identity;
using Movies.Api.Endpoints.Movies;
using Movies.Api.Endpoints.Ratings;

namespace Movies.Api.Endpoints
{
    public static class EndpointsExtensions
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapMovieEndpoints();
            app.MapRatingEndpoints();
            app.MapGenreEndpoints();
            app.MapIdentityEndpoints();
            return app;
        }
    }
}
