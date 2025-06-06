﻿namespace Movies.Api.Endpoints.Movies
{
    public static class MovieEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGetMovie();
            app.MapCreateMovie();
            app.MapGetMovies();
            app.MapUpdateMovie();
            app.MapDeleteMovie();
            return app;
        }
    }
}
