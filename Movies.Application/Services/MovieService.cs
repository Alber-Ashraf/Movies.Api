using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public Task<bool> CreateAsync(Movie movie)
        {
            return _movieRepository.CreateAsync(movie);
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            return _movieRepository.GetAllAsync();
        }

        public Task<Movie> GetByIdAsync(Guid id)
        {
            return _movieRepository.GetByIdAsync(id);
        }

        public Task<Movie> GetBySlugAsync(string slug)
        {
            return _movieRepository.GetBySlugAsync(slug);
        }

        public async Task<Movie?> UpdateAsync(Movie movie)
        {
            // Check if the movie exists before updating
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id);
            if (!movieExists)
            {
                return null; // or throw an exception, depending on your design choice
            }
            // Validate the movie object here if needed
            await _movieRepository.UpdateAsync(movie);
            return movie;
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return _movieRepository.DeleteAsync(id);
        }
    }
}
