using Dapper;
using Movies.Application.DataBase;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QueryFirstOrDefaultAsync<AppUser>(new CommandDefinition("""
                SELECT id AS Id, email AS Email, fullname AS FullName, passwordhash AS PasswordHash, role AS Role, createdat AS CreatedAt
                FROM users
                WHERE LOWER(email) = LOWER(@email);
                """, new { email }, cancellationToken: token));
        }

        public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QueryFirstOrDefaultAsync<AppUser>(new CommandDefinition("""
                SELECT id AS Id, email AS Email, fullname AS FullName, passwordhash AS PasswordHash, role AS Role, createdat AS CreatedAt
                FROM users
                WHERE id = @id;
                """, new { id }, cancellationToken: token));
        }

        public async Task<bool> CreateAsync(AppUser user, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var rows = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO users (id, email, fullname, passwordhash, role, createdat)
                VALUES (@Id, LOWER(@Email), @FullName, @PasswordHash, @Role, @CreatedAt);
                """, user, cancellationToken: token));
            return rows > 0;
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM users WHERE LOWER(email) = LOWER(@email));
                """, new { email }, cancellationToken: token));
        }
    }
}
