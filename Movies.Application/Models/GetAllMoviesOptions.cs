using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Models
{
    public class GetAllMoviesOptions
    {
        public string? Title { get; set; }
        public int? YearOfRelease { get; set; }
        public Guid? UserId { get; set; }
    }
}
