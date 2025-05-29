using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Requests
{
    public class pagedRequest
    {
        public required int Page { get; init; } = 1;
        public required int PageSize { get; init; } = 10;
    }
}
