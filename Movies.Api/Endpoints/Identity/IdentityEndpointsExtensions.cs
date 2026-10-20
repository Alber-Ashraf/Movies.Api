namespace Movies.Api.Endpoints.Identity
{
    public static class IdentityEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapTokenEndpoint();
            return app;
        }
    }
}
