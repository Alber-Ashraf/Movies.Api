using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IValidator<Movie> _movieValidator;
        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
        }
        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            // Validate the movie object using the validator
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
            return await _movieRepository.CreateAsync(movie, token);
        }

        public Task<IEnumerable<Movie>> GetAllAsync(Guid? userid = default, CancellationToken token = default)
        {
            return _movieRepository.GetAllAsync(userid, token);
        }

        public Task<Movie> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken token = default)
        {
            return _movieRepository.GetByIdAsync(id, userid, token);
        }

        public Task<Movie> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken token = default)
        {
            return _movieRepository.GetBySlugAsync(slug, userid, token);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userid = default, CancellationToken token = default)
        {
            // Validate the movie object using the validator
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
            // Check if the movie exists before updating
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);
            if (!movieExists)
            {
                return null; // or throw an exception, depending on your design choice
            }
            // Validate the movie object here if needed
            await _movieRepository.UpdateAsync(movie, userid, token);
            return movie;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
        {
            return _movieRepository.DeleteAsync(id, token);
        }
    }
}
