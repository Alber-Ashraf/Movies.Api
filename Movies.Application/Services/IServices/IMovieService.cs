using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Services.IServices
{
    public interface IMovieService
    {
        public Task<bool> CreateAsync(Movie movie, CancellationToken token = default);
        public Task<Movie> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default);
        public Task<Movie> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default);
        public Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default);
        public Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
        public Task<Movie?> UpdateAsync(Movie movie, Guid? userid = default, CancellationToken token = default);
        public Task<int> GetCountAsync(string? title, int? yaerOfRealease, CancellationToken token = default);

    }
}
