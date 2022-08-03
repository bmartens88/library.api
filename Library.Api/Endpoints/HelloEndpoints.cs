using Library.Api.Endpoints.Internal;

namespace Library.Api.Endpoints;

public class HelloEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("hello", () => "Hello World");
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}