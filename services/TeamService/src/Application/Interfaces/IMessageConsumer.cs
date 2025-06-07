namespace Application.Interfaces;

public interface IMessageConsumer
{
    void Subscribe<T>(string queue, Func<T, Task> handler);
}