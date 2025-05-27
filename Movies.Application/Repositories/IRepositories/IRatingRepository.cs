using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;

namespace Movies.Application.Repositories.IRepositories
{
    public interface IRatingRepository
    {
        Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken token = default);
        Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default);
        Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
        Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
        Task<IEnumerable<MovieRating>> movieRatingsAsync(Guid userId, CancellationToken token = default);
    }
}
