using System.Threading.Tasks;
using RabbitMQ.Client;

namespace CleanArchitecture.Domain.Rabbitmq.Actions;

public sealed class BindQueueToExchange : IRabbitMqAction
{
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;

    public BindQueueToExchange(string queueName, string exchangeName, string routingKey = "")
    {
        _exchangeName = exchangeName;
        _routingKey = routingKey;
        _queueName = queueName;
    }

    public async Task Perform(IChannel channel)
    {
        await channel.QueueBindAsync(_queueName, _exchangeName, _routingKey);
    }
}