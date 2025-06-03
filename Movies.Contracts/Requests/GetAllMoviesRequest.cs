using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Requests
{
    public class GetAllMoviesRequest : pagedRequest
    {
        public string? Title {  get; init; }
        public int? Year {  get; init; }
        public string? SortBy { get; init; }
    }
}
