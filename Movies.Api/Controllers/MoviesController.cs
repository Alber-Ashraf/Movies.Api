using Microsoft.AspNetCore.Mvc;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Models;
using Movies.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Movies.Api.Auth;
using Movies.Application.Services.IServices;
using Asp.Versioning;
using Microsoft.AspNetCore.OutputCaching;


namespace Movies.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IOutputCacheStore _outputCacheStore;
        public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
        {
            _movieService = movieService;
            _outputCacheStore = outputCacheStore;
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        //[ServiceFilter(typeof(ApiKeyAuthFiltter))]
        [HttpPost(ApiEndpoints.Movies.Create)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, CancellationToken token)
        {
            var movie  = request.MapToMovie();
            // Evict the cache for movies tag
            await _outputCacheStore.EvictByTagAsync("movies", token);
            await _movieService.CreateAsync(movie, token);
            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
        [OutputCache(PolicyName = "MovieCache")]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetByIdAsync(id, userId, token)
                : await _movieService.GetBySlugAsync(idOrSlug, userId, token);

            if (movie == null)
            {
                return NotFound();
            }
            var response = movie.MapToResponse();
            return Ok(response);
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        [OutputCache(PolicyName = "MovieCache")]
        //[ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "title", "year", "sortBy", "page", "pageSize" }, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(PagedResponse<MovieResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetAllMoviesRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var options = request.MapToOptions()
                .WithUser(userId);

            var movies = await _movieService.GetAllAsync(options, token);
            var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
            var moviesResponse = movies.MapToResponse(request.Page, request.PageSize, movieCount);
            return Ok(moviesResponse);
        }

        [Authorize(AuthConstants.TrustedMenmberPolicyName)]
        [HttpPut(ApiEndpoints.Movies.Update)]
        [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var movie = request.MapToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
            if (updatedMovie is null)
            {
                return NotFound();
            }
            // Evict the cache for movies tag
            await _outputCacheStore.EvictByTagAsync("movies", token);
            var response = updatedMovie.MapToResponse();
            return Ok(response);
        }

        [Authorize(AuthConstants.TrustedMenmberPolicyName)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
        {
            var deleted = await _movieService.DeleteAsync(id, token);
            if (!deleted)
            {
                return NotFound();
            }
            // Evict the cache for movies tag
            await _outputCacheStore.EvictByTagAsync("movies", token);
            return Ok();
        }
    }
}
