using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services.IServices;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers.V1
{
    [ApiController]
    public class RatingController : Controller
    {
        // Inject the rating service
        private readonly IRatingService _ratingService;
        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }
        [Authorize]
        [HttpPut(ApiEndpoints.V1.Movies.Rate)]
        public async Task<IActionResult> RateMovie([FromRoute] Guid id,
        [FromBody] RateMovieRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var result = await _ratingService.RateMovieAsync(id, userId!.Value, request.Rating, token);
            return result ? Ok() : NotFound();
        }
        [Authorize]
        [HttpDelete(ApiEndpoints.V1.Movies.DeleteRating)]
        public async Task<IActionResult> DeleteRating([FromRoute] Guid id,
            CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var result = await _ratingService.DeleteRatingAsync(id, userId!.Value, token);
            return result ? Ok() : NotFound();
        }
        [Authorize]
        [HttpGet(ApiEndpoints.V1.Ratings.GetUserRating)]
        public async Task<IActionResult> GetUserRatings(CancellationToken token = default)
        {
            var userId = HttpContext.GetUserId();
            var ratings = await _ratingService.movieRatingsAsync(userId!.Value, token);
            var ratingsResponse = ratings.MapToResponse();
            return Ok(ratingsResponse);
        }
    }
}