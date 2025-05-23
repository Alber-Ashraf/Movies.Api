﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Responses
{
    public class MoviesResponse
    {
        public required IEnumerable<MovieResponse> Movies { get; init; } = Enumerable.Empty<MovieResponse>();
    }
}
