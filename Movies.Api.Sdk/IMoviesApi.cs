using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Movies.Contracts.Responses;
using Refit;

namespace Movies.Api.Sdk
{
    public interface IMoviesApi
    {
        [Get(ApiEndpoints.Movies.Get)]
        Task<MovieResponse> GetMovieAsync(string idOrSlug);
    }
}
