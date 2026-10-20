using Movies.Application.Services.IServices;

namespace Movies.Api.Endpoints.Genres
{
    public static class GetGenresEndpoint
    {
        public const string Name = "GetGenres";
        public static IEndpointRouteBuilder MapGetGenres(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.Genres.GetAll, async (
                IMovieService movieService, CancellationToken token) =>
            {
                var genres = await movieService.GetAllGenresAsync(token);
                return TypedResults.Ok(genres);
            })
                .WithName(Name)
                .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
                .CacheOutput("MovieCache");

            return app;
        }
    }
}
