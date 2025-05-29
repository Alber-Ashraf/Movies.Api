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
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public MovieRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public readonly List<Movie> _movie = new();

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = await connection.BeginTransactionAsync();

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES (@Id, @Slug, @Title, @YearOfRelease);
                """, movie, cancellationToken: token));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT INTO genres (movieId, name)
                        VALUES (@MovieId, @Name);
                        """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
                }
            }

            transaction.Commit();
            return result > 0;
        }
        public async Task<Movie> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating
                FROM movies m
                LEFT JOIN ratings r ON m.id = r.movieId
                LEFT JOIN ratings myr ON m.id = myr.movieId
                    and myr.userId = @userid
                WHERE id = @Id
                group by id, userRating;
                """, new { id, userid }, cancellationToken: token));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name FROM genres WHERE movieId = @Id;
                """, new { id }, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }
        public async Task<Movie> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating
                FROM movies m
                LEFT JOIN ratings r ON m.id = r.movieId
                LEFT JOIN ratings myr ON m.id = myr.movieId
                    and myr.userId = @userid
                WHERE slug = @Slug
                group by id, userRating;
                """, new { slug, userid }, cancellationToken: token));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name FROM genres WHERE movieId = @Id;
                """, new { id = movie.Id }, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }
        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var orderClause = string.Empty;
            if (options.SortField is not null)
            {
                orderClause = $"""
                    , m.{options.SortField}
                    order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                """;
            }

            var result = await connection.QueryAsync(new CommandDefinition($"""
                select m.* ,
                    string_agg(g.name, ',') as genres
                    , round(avg(r.rating), 1) as rating
                    , myr.rating as userRating
                from movies m
                left join genres g on m.Id = g.movieId
                left join ratings r on m.id = r.movieId
                left join ratings myr on m.id = myr.movieId
                    and myr.userId = @userid
                where (@title is null or m.title like ('%' || @title || '%'))
                and  (@yearofrelease is null or m.yearofrelease = @yearofrelease)
                group by id, userrating{orderClause}
                limit @pageSize
                offset @pageOffset;
                """, new
                        {
                            userid = options.UserId,
                            title = options.Title,
                            yearofrelease = options.YearOfRelease,
                            pageSize = options.PageSize,
                            pageOffset = (options.Page - 1) * options.PageSize

            }, cancellationToken: token));

            return result.Select(x => new Movie
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Rating = (float?)x.rating,
                UserRating = (int?)x.userrating,
                Genres = Enumerable
                    .ToList(x.genres?.Split(','))
            });
        }
        public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = await connection.BeginTransactionAsync();

            // First, delete existing genres for the movie
            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @id;
                """, new { id = movie.Id }, cancellationToken: token));

            // Then, insert the updated genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
            }
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE movies
                SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                WHERE id = @Id;
                """, movie, cancellationToken: token));
            transaction.Commit();
            return result > 0;
        }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
        {
            var connection = _dbConnectionFactory.CreateConnectionAsync(token).Result;
            using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @Id;
                """, new { id }, cancellationToken: token));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM movies WHERE id = @Id;
                """, new { id }, cancellationToken: token));
            transaction.Commit();
            return result > 0;
        }
        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);
                """, new { id }, cancellationToken: token));
        }

        public async Task<int> GetCountAsync(string? title, int? yaerOfRealease, CancellationToken token = default)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                SELECT COUNT(id)
                FROM movies
                WHERE (@title IS NULL OR title LIKE ('%' || @title || '%'))
                AND (@yaerOfRealease IS NULL OR yearofrelease = @yaerOfRealease);
                """, new 
            { 
                title,
                yaerOfRealease
            }, cancellationToken: token));
        }
    }
}
