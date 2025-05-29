using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators
{
    public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
    {
        public string[] AcceptedSortFields = new[]
        {
            "title","yearofrelease"
        };

        public GetAllMoviesOptionsValidator()
        {
            RuleFor(x => x.YearOfRelease)
                .LessThanOrEqualTo(DateTime.UtcNow.Year);

            RuleFor(x => x.SortField)
                .Must(x => x is null || AcceptedSortFields.Contains(x, StringComparer.Ordinal))
                .WithMessage($"Sort field must be one of the following: {string.Join(", ", AcceptedSortFields)}");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .ExclusiveBetween(1, 25)
                .WithMessage("You can get between 1 and 25 movie per page");
        }
    }
}
