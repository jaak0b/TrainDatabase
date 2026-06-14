namespace TrainDatabase.Core.Domain;

/// <summary>Base type for persisted domain objects carrying a database identity.</summary>
public abstract class BaseObject
{
    public int Id { get; set; }
}
