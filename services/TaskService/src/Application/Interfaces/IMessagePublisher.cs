namespace Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message);
}