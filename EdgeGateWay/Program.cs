using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
// builder.Services.AddHostedService<EdgeGatewayAgent>();
builder.Services.AddLogging();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();
app.MapControllers();


app.MapGet("/", () => "EdgeGatewayAgent is running");

app.Run();

// Models
public class CameraEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CameraProfile
{
    public string CameraId { get; set; } = string.Empty;
    public List<string> InterestedParties { get; set; } = new();
}

// Interfaces
public interface IRabbitMQPublisher
{
    Task PublishEventAsync(CameraEvent cameraEvent, IDictionary<string, object> headers);
}

// RabbitMQ Publisher
public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private const string ExchangeName = "camera_events";

    public RabbitMQPublisher(ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        
var factory = new ConnectionFactory()
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit",
    Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
    UserName = "guest",
    Password = "guest",
    DispatchConsumersAsync = true
};

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare exchange
        _channel.ExchangeDeclare(exchange: ExchangeName, 
                                    type: ExchangeType.Headers, 
                                    durable: true);
        
        _logger.LogInformation("RabbitMQ Publisher initialized");
    }

    public async Task PublishEventAsync(CameraEvent cameraEvent, IDictionary<string, object> headers)
    {
        var message = JsonSerializer.Serialize(cameraEvent);
        var body = Encoding.UTF8.GetBytes(message);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.Headers    = headers;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: string.Empty, // ignored for headers exchange
            basicProperties: props,
            body: body);

        _logger.LogInformation($"Published event with headers [{string.Join(", ", headers.Keys)}]: {message}");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

