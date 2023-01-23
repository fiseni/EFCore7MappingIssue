using AutoMapper;
using AutoMapper.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EFCore7MappingIssue.Common;

public static class AutomapperExtensions
{
    public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddAutoMapper(GetConfigAction(), assemblies);

        return services;
    }

    public static MapperConfiguration CreateConfiguration(params Assembly[] assemblies)
    {
        return new MapperConfiguration(GetConfigAction(assemblies));
    }

    private static Action<IMapperConfigurationExpression> GetConfigAction(params Assembly[] assemblies)
    {
        return cfg =>
        {
            cfg.Internal().ForAllMaps((tm, me) => me.IgnoreAllPropertiesWithAnInaccessibleSetter());
            cfg.ShouldMapField = x => x.IsPublic;
            cfg.ShouldUseConstructor = x => x.IsPublic;
            cfg.ShouldMapMethod = x => false;

            if (assemblies.Any())
            {
                cfg.AddMaps(assemblies);
            }
        };
    }
}
