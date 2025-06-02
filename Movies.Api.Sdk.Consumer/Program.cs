using System.Text.Json;
using System.Threading.Tasks;
using Refit;

namespace Movies.Api.Sdk.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var moviesApi = RestService.For<IMoviesApi>("https://localhost:7252");
            var movie = await moviesApi.GetMovieAsync("father-of-the-bride-part-ii2-1995");
            Console.WriteLine(JsonSerializer.Serialize(movie));
            Console.ReadKey();
        }
    }
}
