using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        public readonly List<Movie> _movie = new();
        
        public Task<bool> CreateAsync(Movie movie)
        {
            _movie.Add(movie);
            return Task.FromResult(true);
        }
        public Task<Movie> GetByIdAsync(Guid id)
        {
            var movie = _movie.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(movie);
        }
        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            var movies = _movie.AsEnumerable();
            return Task.FromResult(movies);
        }
        public Task<bool> UpdateAsync(Movie movie)
        {
            var movieIndex = _movie.FindIndex(x => x.Id == movie.Id);
            if (movieIndex == -1)
            {
                return Task.FromResult(false);
            }
            _movie[movieIndex] = movie;
            return Task.FromResult(true);
        }
        public Task<bool> DeleteAsync(Guid id)
        {
            var removedCount = _movie.RemoveAll(x => x.Id == id);
            var removed = removedCount > 0;
            return Task.FromResult(removed);
        }
    }
}
