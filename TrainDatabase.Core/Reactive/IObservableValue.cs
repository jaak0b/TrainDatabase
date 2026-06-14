namespace TrainDatabase.Core.Reactive;

/// <summary>
/// A read-only reactive value: an observable stream that also exposes its current value.
/// Replaces the read surface of <c>ReactiveProperty&lt;T&gt;</c> without taking a dependency
/// on that library, so the domain and port layers stay framework-agnostic. Subscribers
/// receive the current value on subscription (BehaviorSubject semantics).
/// </summary>
public interface IObservableValue<out T> : IObservable<T>
{
    /// <summary>The current value.</summary>
    T Value { get; }
}
