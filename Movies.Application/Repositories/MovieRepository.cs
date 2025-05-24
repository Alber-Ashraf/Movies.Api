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

        public async Task<bool> CreateAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES (@Id, @Slug, @Title, @YearOfRelease);
                """, movie));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT INTO genres (movieId, name)
                        VALUES (@MovieId, @Name);
                        """, new { MovieId = movie.Id, Name = genre }));
                }
            }

            transaction.Commit();
            return result > 0;
        }
        public async Task<Movie> GetByIdAsync(Guid id)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                SELECT * FROM movies WHERE id = @Id;
                """, new { id }));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name FROM genres WHERE movieId = @Id;
                """, new { id }));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }
        public async Task<Movie> GetBySlugAsync(string slug)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                SELECT * FROM movies WHERE slug = @slug;
                """, new { slug }));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT name FROM genres WHERE movieId = @Id;
                """, new { id = movie.Id }));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }
        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var result = await connection.QueryAsync(new CommandDefinition("""
                select m.* , string_agg(g.name, ',') as genres
                from movies m left join genres g on m.Id = g.movieId
                group by id;
                """));

            return result.Select(x => new Movie
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Genres = Enumerable
                    .ToList(x.genres?.Split(','))
            });
        }
        public async Task<bool> UpdateAsync(Movie movie)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            // First, delete existing genres for the movie
            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @id;
                """, new { id = movie.Id }));

            // Then, insert the updated genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre }));
            }
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE movies
                SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                WHERE id = @Id;
                """, movie));
            transaction.Commit();
            return result > 0;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var connection = _dbConnectionFactory.CreateConnectionAsync().Result;
            using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieId = @Id;
                """, new { id }));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM movies WHERE id = @Id;
                """, new { id }));
            transaction.Commit();
            return result > 0;
        }
        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            var connection = await _dbConnectionFactory.CreateConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);
                """, new { id }));
        }
    }
}
