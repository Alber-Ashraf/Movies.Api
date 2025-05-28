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
        public GetAllMoviesOptionsValidator()
        {
            RuleFor(x => x.YearOfRelease)
                .LessThanOrEqualTo(DateTime.UtcNow.Year);
        }
    }
}
