using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Application.Validators
{
    public class MovieValidator : AbstractValidator<Movie>
    {
        private readonly IMovieRepository _movieRepository;

        public MovieValidator(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;

            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.");

            RuleFor(x => x.Genres)
                .NotEmpty()
                .WithMessage("At least one genre is required.");

            RuleFor(x => x.YearOfRelease)
                .LessThanOrEqualTo(DateTime.Now.Year)
                .WithMessage($"Year of release must not be int the Future.");

            RuleFor(x => x.Slug).MustAsync(ValidateSlug);

        }

        private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
        {
            var existingMovie = await _movieRepository.GetBySlugAsync(slug);
            if (existingMovie is not null)
            {
                return existingMovie.Id == movie.Id; 
            }
            return existingMovie is null; 
        }
    }
}
