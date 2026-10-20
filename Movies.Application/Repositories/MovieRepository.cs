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

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            await using var transaction = await connection.BeginTransactionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES (@Id, @Slug, @Title, @YearOfRelease);
                """, movie, transaction: transaction, cancellationToken: token));

            if (result > 0 && movie.Genres.Count > 0)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);
                    """, movie.Genres.Select(g => new { MovieId = movie.Id, Name = g }), transaction: transaction, cancellationToken: token));
            }

            await transaction.CommitAsync(token);
            return result > 0;
        }
        public async Task<Movie?> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
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
        public async Task<Movie?> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
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
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var sortField = options.SortField?.ToLowerInvariant() switch
            {
                "title" => "title",
                "yearofrelease" => "yearofrelease",
                _ => null
            };

            var orderClause = sortField is not null
                ? $", m.{sortField} order by m.{sortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}"
                : "order by m.id asc";

            var result = await connection.QueryAsync(new CommandDefinition($"""
                select m.* ,
                    string_agg(distinct g.name, ',') as genres
                    , round(avg(r.rating), 1) as rating
                    , myr.rating as userRating
                from movies m
                left join genres g on m.Id = g.movieId
                left join ratings r on m.id = r.movieId
                left join ratings myr on m.id = myr.movieId
                    and myr.userId = @userid
                where (@title is null or m.title ilike ('%' || @title || '%'))
                and  (@yearofrelease is null or m.yearofrelease = @yearofrelease)
                and  (@genre is null or exists(select 1 from genres g2 where g2.movieId = m.id and g2.name ilike @genre))
                group by id, userrating{orderClause}
                limit @pageSize
                offset @pageOffset;
                """, new
                        {
                            userid = options.UserId,
                            title = options.Title,
                            yearofrelease = options.YearOfRelease,
                            genre = options.Genre,
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
                    .ToList(x.genres?.Split(',') ?? Array.Empty<string>())
            });
        }
        public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            await using var transaction = await connection.BeginTransactionAsync(token);

            // First, delete existing genres for the movie
            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @id;
                """, new { id = movie.Id }, transaction: transaction, cancellationToken: token));

            // Then, insert the updated genres
            if (movie.Genres.Count > 0)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);
                    """, movie.Genres.Select(g => new { MovieId = movie.Id, Name = g }), transaction: transaction, cancellationToken: token));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE movies
                SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                WHERE id = @Id;
                """, movie, transaction: transaction, cancellationToken: token));

            await transaction.CommitAsync(token);
            return result > 0;
        }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            await using var transaction = await connection.BeginTransactionAsync(token);

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @Id;
                """, new { id }, transaction: transaction, cancellationToken: token));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM movies WHERE id = @Id;
                """, new { id }, transaction: transaction, cancellationToken: token));

            await transaction.CommitAsync(token);
            return result > 0;
        }
        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);
                """, new { id }, cancellationToken: token));
        }

        public async Task<int> GetCountAsync(string? title, int? yearOfRelease, string? genre = default, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                SELECT COUNT(id)
                FROM movies m
                WHERE (@title IS NULL OR m.title ILIKE ('%' || @title || '%'))
                AND (@yearOfRelease IS NULL OR m.yearofrelease = @yearOfRelease)
                AND (@genre IS NULL OR EXISTS(SELECT 1 FROM genres g WHERE g.movieId = m.id AND g.name ILIKE @genre));
                """, new
            {
                title,
                yearOfRelease,
                genre
            }, cancellationToken: token));
        }

        public async Task<IEnumerable<string>> GetAllGenresAsync(CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT DISTINCT name FROM genres ORDER BY name ASC;
                """, cancellationToken: token));
        }
    }
}
