namespace Movies.Api.Endpoints.Genres
{
    public static class GenreEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapGenreEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGetGenres();
            return app;
        }
    }
}
