using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Responses
{
    public class PagedResponse<TResponse>
    {
        public required IEnumerable<TResponse> Movies { get; init; } = Enumerable.Empty<TResponse>();

        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int TotalCount { get; init; }
        public bool HasNextPage => Page * PageSize < TotalCount;
    }
}
