namespace TrainDatabase.Presentation.ViewModels;

/// <summary>A short summary of a vehicle for list/selection UI.</summary>
public sealed record VehicleListItem(int Id, string Name, long Address)
{
    public string Display => $"#{Address} — {(string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name)}";
}
