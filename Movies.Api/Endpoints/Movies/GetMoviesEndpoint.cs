﻿using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Application.Services.IServices;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies
{
    public static class GetMoviesEndpoint
    {
        public static string Name = "GetMovies";
        public static IEndpointRouteBuilder MapGetMovies(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters]GetAllMoviesRequest request , IMovieService movieService
                , HttpContext context, CancellationToken token) =>
            {
                var userId = context.GetUserId();
                var options = request.MapToOptions()
                    .WithUser(userId);

                var movies = await movieService.GetAllAsync(options, token);

                var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                var moviesResponse = movies.MapToResponse(request.Page.GetValueOrDefault(pagedRequest.DefaultPage), request.PageSize.GetValueOrDefault(pagedRequest.DefaultPageSize), movieCount);
                return TypedResults.Ok(moviesResponse);
            })
                .WithName(Name)
                .Produces<MoviesResponse>(StatusCodes.Status200OK)
                .WithApiVersionSet(ApiVersioning.VersionSet)
                .HasApiVersion(1.0)
                .CacheOutput("MovieCache");
            return app;
        }
    }
}
