using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory factory;
IConnection connection = null;
IChannel channel = null;
AsyncEventingBasicConsumer consumer;

factory = new ConnectionFactory() { HostName = "localhost" };
await Reconnect();

Console.ReadLine();

async Task Connect()
{
    connection = await factory.CreateConnectionAsync();
    connection.ConnectionShutdownAsync += Connection_ConnectionShutdownAsync;

    channel = await connection.CreateChannelAsync();
    await channel.QueueDeclareAsync("testqueue", true, false, false, null);

    consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += Consumer_ReceivedAsync;
    await channel.BasicConsumeAsync("testqueue", true, consumer);
}

async Task CleanUp()
{
    try
    {
        if (channel != null && channel.IsOpen)
        {
            await channel.CloseAsync();
            channel = null;
        }

        if(connection != null && connection.IsOpen)
        {
            await connection.CloseAsync();
            connection = null;
        }
    }
    catch(IOException ex)
    {
        // Close() may throw an IOException if connection dies (handled by reconnect)
    }
}

async Task Reconnect()
{
    await CleanUp();

    var mres = new ManualResetEvent(false);

    while(!mres.WaitOne(3000))
    {
        try
        {
            await Connect();

            Console.WriteLine("Connected");
            mres.Set();
        }
        catch(Exception ex)
        {
            Console.WriteLine("Connection failed");
        }
    }
}

async Task Connection_ConnectionShutdownAsync(object sender, ShutdownEventArgs ev)
{
    Console.WriteLine("Connection lost");
    await Reconnect();
}

async Task<string?> Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs ev)
{
    var body = ev.Body;
    var content = Encoding.UTF8.GetString(body.ToArray());
    Console.WriteLine(content);

    return content;
}