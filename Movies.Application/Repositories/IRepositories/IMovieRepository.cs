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
        public Task<bool> CreateAsync(Movie movie);
        public Task<Movie> GetByIdAsync(Guid id);
        public Task<Movie> GetBySlugAsync(string slug);
        public Task<IEnumerable<Movie>> GetAllAsync();
        public Task<bool> DeleteAsync(Guid id);
        public Task<bool> UpdateAsync(Movie movie);
    }
}
