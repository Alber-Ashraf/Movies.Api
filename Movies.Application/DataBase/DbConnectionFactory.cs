using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Movies.Application.DataBase
{
    class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<DbConnection> CreateConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
