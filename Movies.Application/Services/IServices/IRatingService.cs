using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services.IServices
{
    public interface IRatingService
    {
        Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken token = default);
        Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
    }
}
