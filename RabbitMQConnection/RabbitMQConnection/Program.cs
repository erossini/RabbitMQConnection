using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };

using (var connection = await factory.CreateConnectionAsync())
{
    using (var channel = await connection.CreateChannelAsync())
    {
        await channel.QueueDeclareAsync("testqueue", true, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += Consumer_ReceivedAsync;
        await channel.BasicConsumeAsync("testqueue", true, consumer);

        Console.ReadLine();
    }
}

async Task<string?> Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs ev)
{
    var body = ev.Body;
    var content = Encoding.UTF8.GetString(body.ToArray());
    Console.WriteLine(content);

    return content;
}