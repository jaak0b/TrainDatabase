using Mapster;

namespace TrainDatabase.Infrastructure.Mapping;

/// <summary>
/// Mapster-backed <see cref="IEntityMapper"/>. Holds a compiled <see cref="TypeAdapterConfig"/>
/// (built by <see cref="MappingConfig.Create"/>) and adapts through it.
/// </summary>
public sealed class MapsterEntityMapper(TypeAdapterConfig config) : IEntityMapper
{
    public TDestination Map<TDestination>(object source) => source.Adapt<TDestination>(config);

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
        source.Adapt(destination, config);
}
