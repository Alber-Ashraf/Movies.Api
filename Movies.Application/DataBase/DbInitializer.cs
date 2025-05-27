using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Movies.Application.DataBase
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            //await connection.ExecuteAsync("""
                
            //    Drop table ratings;
            //    Drop table genres;
            //    Drop table movies;

            //    """);
            await connection.ExecuteAsync("""
                
                CREATE TABLE IF NOT EXISTS movies (
                    id UUID PRIMARY KEY,
                    slug TEXT NOT NULL,
                    title TEXT NOT NULL,
                    yearofrelease INTEGER NOT NULL
                );
                
                """);
            await connection.ExecuteAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS movies_slug_idx ON movies (slug);
                
                """);

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS genres (
                    movieId UUID REFERENCES movies (id) ON DELETE CASCADE,
                    name TEXT NOT NULL
                );

                """);

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS ratings (
                    userId UUID,
                    movieId UUID REFERENCES movies (id) ON DELETE CASCADE,
                    rating INTEGER NOT NULL,
                    PRIMARY KEY (userId, movieId)
                );
                
                """);
        }
    }
}
