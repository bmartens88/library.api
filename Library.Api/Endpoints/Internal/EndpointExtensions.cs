﻿using System.Reflection;

namespace Library.Api.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void AddEndpoints(this IServiceCollection services, Type typeMarker, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, new object[] {services, configuration});
    }

    public static void AddEndpoints<TMarker>(this IServiceCollection services, IConfiguration configuration)
    {
        AddEndpoints(services, typeof(TMarker), configuration);
    }

    public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, new object[] {app});
    }

    public static void UseEndPoints<TMarker>(this IApplicationBuilder app)
    {
        UseEndpoints(app, typeof(TMarker));
    }

    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        return typeMarker.Assembly.DefinedTypes
            .Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x));
    }
}