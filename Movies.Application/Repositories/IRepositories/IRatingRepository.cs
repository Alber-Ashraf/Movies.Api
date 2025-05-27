using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Repositories.IRepositories
{
    public interface IRatingRepository
    {
        Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default);
        Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
    }
}
