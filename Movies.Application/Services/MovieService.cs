using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Services.IServices;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IValidator<Movie> _movieValidator;
        private readonly IRatingRepository _ratingRepository;
        private readonly IValidator<GetAllMoviesOptions> _optionsValidator;
        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
            _ratingRepository = ratingRepository;
            _optionsValidator = optionsValidator;
        }
        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            // Validate the movie object using the validator
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
            return await _movieRepository.CreateAsync(movie, token);
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken: token);

            return await _movieRepository.GetAllAsync(options, token);
        }

        public Task<Movie> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default)
        {
            return _movieRepository.GetByIdAsync(id, userid, token);
        }

        public Task<Movie> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default)
        {
            return _movieRepository.GetBySlugAsync(slug, userid, token);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default,  CancellationToken token = default)
        {
            // Validate the movie object using the validator
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
            // Check if the movie exists before updating
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);
            if (!movieExists)
            {
                return null; // or throw an exception, depending on your design choice
            }
            // Validate the movie object here if needed
            await _movieRepository.UpdateAsync(movie, token);

            if(!userId.HasValue){
                var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
                movie.Rating = rating;
                return movie;
            }

            var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, token);
            movie.Rating = ratings.Rating;
            movie.UserRating = ratings.UserRating;
            return movie;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
        {
            return _movieRepository.DeleteAsync(id, token);
        }

        public async Task<int> GetCountAsync(string? title, int? yaerOfRealease, CancellationToken token = default)
        {
           return await _movieRepository.GetCountAsync(title, yaerOfRealease, token);
        }
    }
}
