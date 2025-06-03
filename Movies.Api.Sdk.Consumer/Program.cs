using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("https://localhost:7252");

var service = new ServiceCollection();
service.AddRefitClient<IMoviesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7252"));

var serviceProvider = service.BuildServiceProvider();
var moviesApi = serviceProvider.GetRequiredService<IMoviesApi>();

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

foreach (var movieResponse in allMovies.Movies)
{
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}

Console.ReadKey();
