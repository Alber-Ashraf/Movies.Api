using System.Text.Json;
using System.Threading.Tasks;
using Movies.Contracts.Requests;
using Refit;

namespace Movies.Api.Sdk.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var moviesApi = RestService.For<IMoviesApi>("https://localhost:7252");
            var movie = await moviesApi.GetMovieAsync("father-of-the-bride-part-ii2-1995");
            //Console.WriteLine(JsonSerializer.Serialize(movie));

            var request = new GetAllMoviesRequest
            {
                Title = null,
                Year = null,
                SortBy = null,
                Page = 1,
                PageSize = 3,
            };

            var allMovies = await moviesApi.GetAllMoviesAsync(request);
             
            foreach ( var movieResponse in allMovies.Movies ) {
                Console.WriteLine(JsonSerializer.Serialize(movieResponse));
            }

            Console.ReadKey();
        }
    }
}
