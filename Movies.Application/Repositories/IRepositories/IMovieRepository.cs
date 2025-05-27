using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;

namespace Movies.Application.Repositories.IRepositories
{
    public interface IMovieRepository
    {
        public Task<bool> CreateAsync(Movie movie, CancellationToken token = default);
        public Task<Movie> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default);
        public Task<Movie> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default);
        public Task<IEnumerable<Movie>> GetAllAsync(Guid? userid = default, CancellationToken token = default);
        public Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
        public Task<bool> UpdateAsync(Movie movie, CancellationToken token = default);
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);

    }
}
