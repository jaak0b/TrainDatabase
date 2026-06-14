namespace TrainDatabase.Core.Domain;

/// <summary>
/// Domain constants for DCC (Digital Command Control) addressing.
/// Replaces the former dependency on <c>Z21.Client.maxDccStep</c> so the domain
/// layer carries no hardware-library reference.
/// </summary>
public static class DccConstants
{
    /// <summary>
    /// Highest DCC speed step (128-step mode uses steps 0..127).
    /// </summary>
    public const int MaxDccStep = 127;
}
