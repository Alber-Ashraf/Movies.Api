using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Services.IServices;

namespace Movies.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMovieRepository _movieRepository;

        public RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository)
        {
            _ratingRepository = ratingRepository;
            _movieRepository = movieRepository;
        }

        public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken token = default)
        {
            if (rating < 1 || rating > 5)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure
                    {
                        PropertyName = "Rating",
                        ErrorMessage = "Rating must be between 1 and 5."
                    }
                });
            }

            var movieExists = await _movieRepository.ExistsByIdAsync(movieId, token);
            if (!movieExists)
            {
                return false;
            }

            return await _ratingRepository.RateMovieAsync(movieId, userId, rating, token);
        }

        public Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            return _ratingRepository.DeleteRatingAsync(movieId, userId, token);
        }

        public Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default)
        {
            return _ratingRepository.GetRatingsForUserAsync(userId, token);
        }
    }
}
