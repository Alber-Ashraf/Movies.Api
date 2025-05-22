using Microsoft.AspNetCore.Mvc;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Models;
using Movies.Api.Mapping;

namespace Movies.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;
        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        [HttpPost("Movies")]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
        {
            var movie  = request.MapToMovie();
            await _movieRepository.CreateAsync(movie);
            return Created($"/api/movies/{movie.Id}", movie);
        }
    }
}
