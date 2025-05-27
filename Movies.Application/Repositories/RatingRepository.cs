using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Movies.Application.DataBase;
using Movies.Application.Models;
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
        public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO ratings (movieid, userid, rating)
                    VALUES (@movieId, @userId, @rating)
                    ON CONFLICT (userId, movieId) DO UPDATE
                    SET rating = @rating;
               """, new { movieId, userId, rating }, cancellationToken: token));
            return result > 0;
        }
        public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            select round(avg(r.rating), 1) from ratings r
            where movieid = @movieId
            """, new { movieId }, cancellationToken: token));
        }

        public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
            select round(avg(rating), 1), 
                   (select rating 
                    from ratings 
                    where movieid = @movieId 
                      and userid = @userId
                    limit 1) 
            from ratings
            where movieid = @movieId
            """, new { movieId, userId }, cancellationToken: token));
        }

        public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
            delete from ratings
            where movieid = @movieId
            and userid = @userId
            """, new { userId, movieId }, cancellationToken: token));
            return result > 0;
        }

        public async Task<IEnumerable<MovieRating>> movieRatingsAsync(Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QueryAsync<MovieRating>(new CommandDefinition("""
                select r.rating, r.movieid, m.slug
                from ratings r
                join movies m on r.movieid = m.id
                where r.userid = @userId
                """, new { userId }, cancellationToken: token));
        }
    }
}
