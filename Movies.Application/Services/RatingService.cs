using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Services.IServices;

namespace Movies.Application.Services
{
    class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMovieRepository _movieRepository;
        public RatingService(IRatingRepository ratingRepository, IMovieRepository _movieRepository)
        {
            _ratingRepository = ratingRepository;
            this._movieRepository = _movieRepository;
        }
        public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken token = default)
        {
            // Validate the rating value
            if (rating < 0 || rating > 5)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure
                    {
                        PropertyName = "Rating",
                        ErrorMessage = "Rating must be between 0 and 5."
                    }
                });
            }

            // Validate the movieId to ensure the movie exists
            var movieExists = await _movieRepository.ExistsByIdAsync(movieId, token);
            if (!movieExists)
            {
                return false;
            }
            // Validate the userId if necessary, e.g., check if the user exists
            return await _ratingRepository.RateMovieAsync(movieId, userId, rating, token);
        }
    }
}
