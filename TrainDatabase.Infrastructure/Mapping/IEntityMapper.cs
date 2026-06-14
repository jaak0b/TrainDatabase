namespace TrainDatabase.Infrastructure.Mapping;

/// <summary>
/// Small mapping abstraction over Mapster, used at the repository boundary to convert
/// EF entities ↔ domain models. Keeping this interface (rather than depending on a
/// third-party mapper type directly) makes repositories trivially testable and isolates
/// the rest of the layer from the mapping library.
/// </summary>
public interface IEntityMapper
{
    /// <summary>Maps <paramref name="source"/> to a new instance of <typeparamref name="TDestination"/>.</summary>
    TDestination Map<TDestination>(object source);

    /// <summary>Maps <paramref name="source"/> onto the existing <paramref name="destination"/> and returns it.</summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
