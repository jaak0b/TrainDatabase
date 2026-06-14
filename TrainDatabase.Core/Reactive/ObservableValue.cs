using System.Reactive.Subjects;

namespace TrainDatabase.Core.Reactive;

/// <summary>
/// A mutable <see cref="IObservableValue{T}"/> backed by a <see cref="BehaviorSubject{T}"/>.
/// Producers (e.g. hardware adapters) push updates with <see cref="SetValue"/>; consumers
/// observe the stream and read <see cref="Value"/>. Subscribers receive the current value
/// on subscription. Pushing after disposal is a no-op (adapters may emit during teardown).
/// </summary>
public sealed class ObservableValue<T> : IObservableValue<T>, IDisposable
{
    private readonly BehaviorSubject<T> subject;
    private bool disposed;

    public ObservableValue(T initialValue) => subject = new BehaviorSubject<T>(initialValue);

    public T Value => subject.Value;

    public void SetValue(T value)
    {
        if (disposed)
        {
            return;
        }

        subject.OnNext(value);
    }

    public IDisposable Subscribe(IObserver<T> observer) => subject.Subscribe(observer);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        subject.Dispose();
    }
}
