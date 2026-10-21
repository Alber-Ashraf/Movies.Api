using Movies.Application.Models;

namespace Movies.Application.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken token = default);
        Task<AppUser?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<bool> CreateAsync(AppUser user, CancellationToken token = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default);
    }
}
