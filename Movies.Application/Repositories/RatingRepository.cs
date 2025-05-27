using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Movies.Application.DataBase;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public RatingRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
                    SELECT round(avg(r.rating), 1) as rating
                    FROM ratings r
                    WHERE movieId = @MovieId;
                    """, new { movieId }, cancellationToken: token));
        }

        public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
                    SELECT round(avg(r.rating), 1) as rating,
                    (SELECT rating
                    FROM ratings
                    WHERE movieid = @movieId
                    and userid = @UserId
                    limit 1)
                    FROM ratings r
                    WHERE movieId = @MovieId;
                    """, new { movieId, userId }, cancellationToken: token));
        }
    }
}
