# PoC.CastleWindsorGenericDecorators
Proof of Concept: How to implement open generic decorator chains with Castle Windsor

## Scenario

Register any number of decorators around concrete types that implement a generic interface in Castle Windsor.

## Example

I wanted something simple, so I chose the command pattern and wrapped it with logging. So I have a single generic interface:

```c#
public interface IHandle<in T>
{
    void Handle(T message);
}
```

and one concrete implementor (but this should not be limited to one):

```c#
public class CopyFileHandler : IHandle<CopyFile>
{
    public void Handle(CopyFile message)
    {
        Console.WriteLine("CopyFileHandler.Handle");
    }
}
```

and one decorator for any `IHandle<T>` type (but this should not be limited to one):

```c#
public class LoggingHandler<T> : IHandle<T>
{
    readonly IHandle<T> _decorated;

    public LoggingHandler(IHandle<T> decorated)
    {
        _decorated = decorated;
    }

    public void Handle(T message)
    {
        Console.WriteLine("LoggingHandler.Handle :: Before");
        _decorated.Handle(message);
        Console.WriteLine("LoggingHandler.Handle :: After");
    }
}
```

So when I resolve `IHandle<CopyFile>` from the container I only want to get the decorated handler `LoggingHandler<CopyFile>` that had `CopyFileHandler` passed in to its constructor.

This is not happening with the default Castle Windsor decorator handling.

## Output

```
## Handler 1: LoggingHandler`1
LoggingHandler.Handle :: Before
PlayEventHandler.Handle
LoggingHandler.Handle :: After

## Handler 2: PlayEventHandler
PlayEventHandler.Handle
```
