using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Services
{
    public interface IMovieService
    {
        public Task<bool> CreateAsync(Movie movie);
        public Task<Movie> GetByIdAsync(Guid id);
        public Task<Movie> GetBySlugAsync(string slug);
        public Task<IEnumerable<Movie>> GetAllAsync();
        public Task<bool> DeleteAsync(Guid id);
        public Task<Movie?> UpdateAsync(Movie movie);
    }
}
