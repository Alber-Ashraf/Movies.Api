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

            await connection.ExecuteAsync("""
                
                Create table if not exists movies (
                    id UUID primary key,
                    slug TEXT not null,
                    title TEXT not null,
                    yearofrelease integer not null
                );
                
                """);
            await connection.ExecuteAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS movies_slug_idx ON movies (slug);
                
                """);
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS genres (
                    movieId UUID references movies (Id),
                    name TEXT not null
                );

                """);

        }
    }
}
