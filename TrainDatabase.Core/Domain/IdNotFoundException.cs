namespace TrainDatabase.Core.Domain;

/// <summary>Thrown when a required entity cannot be found for a given id.</summary>
public class IdNotFoundException : Exception
{
    public IdNotFoundException()
    {
    }

    public IdNotFoundException(string message) : base(message)
    {
    }
}
